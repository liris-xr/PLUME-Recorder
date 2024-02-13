using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using PLUME.Core.Recorder.Data;
using PLUME.Core.Recorder.Module;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Core.Recorder.Time;
using PLUME.Core.Utils;
using Unity.Collections;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Pool;

namespace PLUME.Core.Recorder
{
    /// <summary>
    /// The frame recorder is responsible for recording data associated with Unity frames.
    /// It automatically runs after <see cref="PostLateUpdate"/> if <see cref="InjectUpdateInCurrentLoop"/> was called (automatically called by <see cref="PlumeRecorder"/> when the instance is created).
    /// It is responsible for running the <see cref="IFrameDataRecorderModule.RecordFrameData"/> and <see cref="IFrameDataRecorderModuleAsync.RecordFrameDataAsync"/> on <see cref="IFrameDataRecorderModule"/> and <see cref="IFrameDataRecorderModuleAsync"/> modules respectively.
    /// </summary>
    public class FrameRecorder
    {
        /// <summary>
        /// Clock used by the frame recorder to timestamp the frames. The frame recorder is not responsible for starting and stopping the clock (this is typically done by the <see cref="PlumeRecorder"/>).
        /// </summary>
        private readonly IReadOnlyClock _clock;

        private readonly SampleTypeUrlRegistry _typeUrlRegistry;

        private readonly IFrameDataRecorderModule[] _frameDataRecorderModules;
        private readonly IFrameDataRecorderModuleAsync[] _asyncFrameDataRecorderModules;

        private readonly IRecorderData _recorderData;

        private readonly FrameSamplePacker _frameSamplePacker;

        /// <summary>
        /// List of tasks that are currently running and serializing frames data.
        /// Tasks are added in the <see cref="Update"/> method and automatically removed when they finish.
        /// Tasks may queue up if the serialization process is slow. This list is used to wait for all the tasks to finish before stopping the recorder (see <see cref="CompleteTasks"/>).
        /// </summary>
        private readonly HashSet<FrameRecorderTask> _tasks = new(FrameRecorderTaskComparer.Instance);

        /// <summary>
        /// Whether the frame recorder should run the update loop. This is automatically set to true when the recorder starts and false when it stops.
        /// </summary>
        private bool _shouldRunUpdateLoop;

        private CancellationTokenSource _cancellationTokenSource;

        private readonly ObjectPool<List<FrameDataRecorderModuleTask>> _moduleTasksListPool =
            new(() => new List<FrameDataRecorderModuleTask>(20), l => l.Clear());

        private FrameRecorder(IReadOnlyClock clock,
            SampleTypeUrlRegistry typeUrlRegistry,
            IFrameDataRecorderModule[] modules,
            IFrameDataRecorderModuleAsync[] asyncModules,
            FrameSamplePacker frameSamplePacker,
            IRecorderData recorderData)
        {
            _clock = clock;
            _typeUrlRegistry = typeUrlRegistry;
            _frameDataRecorderModules = modules;
            _asyncFrameDataRecorderModules = asyncModules;
            _frameSamplePacker = frameSamplePacker;
            _recorderData = recorderData;
        }

        internal static FrameRecorder Instantiate(IReadOnlyClock clock, SampleTypeUrlRegistry typeUrlRegistry,
            IRecorderModule[] recorderModules, FrameSamplePacker frameSamplePacker, IRecorderData output,
            bool injectUpdateInCurrentLoop)
        {
            var modules = recorderModules.OfType<IFrameDataRecorderModule>().ToArray();
            var asyncModules = recorderModules.OfType<IFrameDataRecorderModuleAsync>().ToArray();
            var frameRecorder =
                new FrameRecorder(clock, typeUrlRegistry, modules, asyncModules, frameSamplePacker, output);

            if (injectUpdateInCurrentLoop)
            {
                frameRecorder.InjectUpdateInCurrentLoop();
            }

            return frameRecorder;
        }

        /// <summary>
        /// Injects the <see cref="Update"/> method in the current player loop. Called automatically by <see cref="PlumeRecorder.Instantiate()"/> when the instance is created.
        /// </summary>
        internal void InjectUpdateInCurrentLoop()
        {
            PlayerLoopUtils.InjectUpdateInCurrentLoop(typeof(FrameRecorder), Update, typeof(PostLateUpdate));
        }

        /// <summary>
        /// Starts the frame recorder. Called automatically by <see cref="PlumeRecorder.Start"/> when the recorder starts.
        /// For internal use only. Use <see cref="PlumeRecorder.Start"/> to start the recorder.
        /// </summary>
        internal void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _shouldRunUpdateLoop = true;
        }

        /// <summary>
        /// Stops the frame recorder. Called automatically by <see cref="PlumeRecorder.Stop"/> when the recorder stops.
        /// For internal use only. Use <see cref="PlumeRecorder.Stop"/> to stop the recorder.
        /// </summary>
        internal async UniTask Stop()
        {
            _shouldRunUpdateLoop = false;
            await CompleteTasks();
        }

        internal void ForceStop()
        {
            var remainingTasksCount = GetRemainingTasksCount();

            _shouldRunUpdateLoop = false;
            _cancellationTokenSource.Cancel();

            if (remainingTasksCount > 0)
            {
                Debug.LogWarning(remainingTasksCount == 1
                    ? "1 frame task was cancelled due to the frame recorder being forced stopped."
                    : $"{remainingTasksCount} frame tasks were cancelled due to the frame recorder being forced stopped.");
            }
        }

        private async void Update()
        {
            if (!_shouldRunUpdateLoop)
                return;

            var timestamp = _clock.ElapsedNanoseconds;
            var frame = UnityEngine.Time.frameCount;
            var task = RecordFrameAsync(timestamp, frame, _recorderData)
                .AttachExternalCancellation(_cancellationTokenSource.Token);
            var recordFrameTask = FrameRecorderTask.Get(frame, task);

            lock (_tasks)
            {
                _tasks.Add(recordFrameTask);
            }

            try
            {
                await task;
            }
            catch (OperationCanceledException)
            {
                // Ignore
            }
            finally
            {
                lock (_tasks)
                {
                    _tasks.Remove(recordFrameTask);
                    FrameRecorderTask.Release(recordFrameTask);
                }
            }
        }

        internal async UniTask RecordFrameAsync(long timestamp, int frameNumber, IRecorderData output)
        {
            var frameDataBuffer = new SerializedSamplesBuffer(Allocator.Persistent);
            await RecordFrameDataAsync(frameDataBuffer);

            var data = new NativeList<byte>(Allocator.TempJob);
            
            var jobHandle =
                _frameSamplePacker.WriteFramePackedSampleAsync(timestamp, frameNumber, _typeUrlRegistry,
                    frameDataBuffer, data);

            try
            {
                await jobHandle.WaitAsync(PlayerLoopTiming.Update, _cancellationTokenSource.Token);
                output.AddTimestampedData(data.AsArray().AsReadOnlySpan(), timestamp);
            }
            catch (OperationCanceledException)
            {
                // Ignored
            }
            finally
            {
                data.Dispose();
                frameDataBuffer.Dispose();
            }
        }

        internal async UniTask RecordFrameDataAsync(SerializedSamplesBuffer serializedSamplesBuffer)
        {
            // Run all the synchronous modules
            foreach (var module in _frameDataRecorderModules)
            {
                var moduleBuffer = new SerializedSamplesBuffer(Allocator.Persistent);
                module.RecordFrameData(moduleBuffer);
                serializedSamplesBuffer.Merge(moduleBuffer);
                moduleBuffer.Dispose();
            }

            // Run all the asynchronous modules
            List<FrameDataRecorderModuleTask> moduleTasks;

            lock (_moduleTasksListPool)
            {
                moduleTasks = _moduleTasksListPool.Get();
            }

            foreach (var module in _asyncFrameDataRecorderModules)
            {
                var moduleBuffer = new SerializedSamplesBuffer(Allocator.Persistent);
                // Fire the task but don't wait for it to finish yet. This is to allow all modules to run their
                // synchronous code first (like querying the object states on main thread) before running their
                // async code (eg. serialization).
                var task = module.RecordFrameDataAsync(moduleBuffer);
                var moduleTask = new FrameDataRecorderModuleTask(task, moduleBuffer);
                moduleTasks.Add(moduleTask);
            }

            foreach (var asyncTask in moduleTasks)
            {
                await asyncTask.Task;
                var moduleBuffer = asyncTask.Buffer;
                serializedSamplesBuffer.Merge(moduleBuffer);
                moduleBuffer.Dispose();
            }

            lock (_moduleTasksListPool)
            {
                _moduleTasksListPool.Release(moduleTasks);
            }
        }

        /// <summary>
        /// Waits synchronously for all the frame recording tasks to finish. This method is called automatically by <see cref="PlumeRecorder.Stop"/> when the recorder stops.
        /// </summary>
        public async UniTask CompleteTasks()
        {
            await UniTask.WaitUntil(() => GetRemainingTasksCount() == 0);
        }

        public int GetRemainingTasksCount()
        {
            lock (_tasks)
            {
                return _tasks.Count;
            }
        }
    }
}