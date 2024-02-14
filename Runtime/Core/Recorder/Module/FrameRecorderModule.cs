using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using PLUME.Core.Recorder.Data;
using PLUME.Core.Utils;
using Unity.Collections;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Pool;

namespace PLUME.Core.Recorder.Module
{
    /// <summary>
    /// The frame recorder is responsible for recording data associated with Unity frames.
    /// It automatically runs after <see cref="PostLateUpdate"/> if <see cref="InjectUpdateInCurrentLoop"/> was called (automatically called by <see cref="Recorder"/> when the instance is created).
    /// It is responsible for running the <see cref="IFrameDataRecorderModule.RecordFrameData"/> and <see cref="IFrameDataRecorderModule.RecordFrameData"/> on <see cref="IFrameDataRecorderModule"/> and <see cref="IFrameDataRecorderModule"/> modules respectively.
    /// </summary>
    public class FrameRecorderModule : IRecorderModule
    {
        private RecorderContext _recorderContext;
        private RecordContext _recordContext;

        // TODO: convert to a utility class?
        private FrameSamplePacker _frameSamplePacker;

        private IFrameDataRecorderModule[] _frameDataRecorderModules;

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

        void IRecorderModule.Create(RecorderContext recorderContext)
        {
            _recorderContext = recorderContext;
            _frameSamplePacker = new FrameSamplePacker();
            _frameDataRecorderModules = recorderContext.Modules.OfType<IFrameDataRecorderModule>().ToArray();
            InjectUpdateInCurrentLoop();
        }

        void IRecorderModule.Start(RecordContext recordContext, RecorderContext recorderContext)
        {
            _recordContext = recordContext;
            _cancellationTokenSource = new CancellationTokenSource();
            _shouldRunUpdateLoop = true;
        }

        async UniTask IRecorderModule.Stop(RecordContext recordContext, RecorderContext recorderContext,
            CancellationToken cancellationToken)
        {
            var remainingTasksCount = GetRemainingTasksCount();
            Debug.Log($"Waiting for {remainingTasksCount} recording tasks to complete.");

            _shouldRunUpdateLoop = false;
            await CompleteTasks(cancellationToken);
            _recordContext = null;
        }

        void IRecorderModule.ForceStop(RecordContext recordContext, RecorderContext recorderContext)
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

        void IRecorderModule.Destroy(RecorderContext recorderContext)
        {
        }

        void IRecorderModule.Reset(RecorderContext context)
        {
        }

        /// <summary>
        /// Injects the <see cref="Update"/> method in the current player loop. Called automatically by <see cref="Recorder.Instantiate()"/> when the instance is created.
        /// </summary>
        internal void InjectUpdateInCurrentLoop()
        {
            PlayerLoopUtils.InjectUpdateInCurrentLoop(typeof(FrameRecorderModule), Update, typeof(PostLateUpdate));
        }

        private async void Update()
        {
            if (!_shouldRunUpdateLoop)
                return;

            var timestamp = _recordContext.Clock.ElapsedNanoseconds;
            var frame = UnityEngine.Time.frameCount;
            var task = RecordFrameAsync(timestamp, frame, _recorderContext.SampleTypeUrlRegistry, _recordContext.Data,
                _cancellationTokenSource.Token);
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

        internal async UniTask RecordFrameAsync(long timestamp, int frameNumber,
            SampleTypeUrlRegistry sampleTypeUrlRegistry, IRecordData output,
            CancellationToken cancellationToken = default)
        {
            var frameDataBuffer = new SerializedSamplesBuffer(Allocator.Persistent);

            try
            {
                await RecordFrameDataAsync(frameDataBuffer, cancellationToken);
                await WritePackedFrameAsync(timestamp, frameNumber, frameDataBuffer, sampleTypeUrlRegistry, output,
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Ignored
            }
            finally
            {
                frameDataBuffer.Dispose();
            }
        }

        private async Task WritePackedFrameAsync(long timestamp, int frameNumber,
            SerializedSamplesBuffer frameDataBuffer, SampleTypeUrlRegistry sampleTypeUrlRegistry,
            IRecordData output, CancellationToken cancellationToken)
        {
            var data = new NativeList<byte>(Allocator.TempJob);

            var jobHandle = _frameSamplePacker.WriteFramePackedSampleAsync(timestamp, frameNumber,
                sampleTypeUrlRegistry, frameDataBuffer, data);

            // Do not catch OperationCanceledException here. The caller should handle it.
            try
            {
                await jobHandle.WaitAsync(PlayerLoopTiming.Update, cancellationToken);
                output.AddTimestampedData(data.AsArray().AsReadOnlySpan(), timestamp);
            }
            finally
            {
                data.Dispose();
            }
        }

        internal async UniTask RecordFrameDataAsync(SerializedSamplesBuffer serializedSamplesBuffer,
            CancellationToken cancellationToken = default)
        {
            List<FrameDataRecorderModuleTask> moduleTasks;

            // TODO: check thread, maybe not required as on main thread
            lock (_moduleTasksListPool)
            {
                moduleTasks = _moduleTasksListPool.Get();
            }

            foreach (var module in _frameDataRecorderModules)
            {
                var moduleBuffer = new SerializedSamplesBuffer(Allocator.Persistent);
                // Fire the task but don't wait for it to finish yet. This is to allow all modules to run their
                // synchronous code first (like querying the object states on main thread) before running their
                // async code (eg. serialization).
                var task = module.RecordFrameData(moduleBuffer);
                var moduleTask = new FrameDataRecorderModuleTask(task, moduleBuffer);
                moduleTasks.Add(moduleTask);
            }

            foreach (var tasks in moduleTasks)
            {
                await tasks.Task;
                var moduleBuffer = tasks.Buffer;
                serializedSamplesBuffer.Merge(moduleBuffer);
                moduleBuffer.Dispose();
            }

            // TODO: check thread, maybe not required as on main thread
            lock (_moduleTasksListPool)
            {
                _moduleTasksListPool.Release(moduleTasks);
            }
        }

        /// <summary>
        /// Waits synchronously for all the frame recording tasks to finish. This method is called automatically by <see cref="Recorder.Stop"/> when the recorder stops.
        /// </summary>
        private async UniTask CompleteTasks(CancellationToken cancellationToken = default)
        {
            await UniTask.WaitUntil(() => GetRemainingTasksCount() == 0, PlayerLoopTiming.Update, cancellationToken);
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