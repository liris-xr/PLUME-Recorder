using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Core.Recorder.Time;
using PLUME.Core.Utils;
using Unity.Collections;
using UnityEngine;
using UnityEngine.PlayerLoop;

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

        private readonly IFrameDataRecorderModule[] _frameDataRecorderModules;
        private readonly IFrameDataRecorderModuleAsync[] _asyncFrameDataRecorderModules;

        /// <summary>
        /// List of tasks that are currently running and serializing frames data.
        /// Tasks may queue up if the serialization process is slow. This list is used to wait for all the tasks to finish before stopping the recorder (see <see cref="CompleteTasks"/>).
        /// </summary>
        private readonly HashSet<FrameRecorderModuleTask> _tasks = new();

        /// <summary>
        /// Whether the frame recorder should run the update loop. This is automatically set to true when the recorder starts and false when it stops.
        /// </summary>
        private bool _shouldRunUpdateLoop;

        public FrameRecorder(IReadOnlyClock clock,
            IFrameDataRecorderModule[] modules,
            IFrameDataRecorderModuleAsync[] asyncModules)
        {
            _clock = clock;
            _frameDataRecorderModules = modules;
            _asyncFrameDataRecorderModules = asyncModules;
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
            _shouldRunUpdateLoop = true;
        }

        /// <summary>
        /// Stops the frame recorder. Called automatically by <see cref="PlumeRecorder.Stop"/> when the recorder stops.
        /// For internal use only. Use <see cref="PlumeRecorder.Stop"/> to stop the recorder.
        /// </summary>
        internal void Stop()
        {
            _shouldRunUpdateLoop = false;
            CompleteTasks();
        }

        private async void Update()
        {
            if (!_shouldRunUpdateLoop)
                return;

            var timestamp = _clock.ElapsedNanoseconds;
            var frame = UnityEngine.Time.frameCount;
            var task = RecordFrameAsync(timestamp, frame);
            var recordFrameTask = new FrameRecorderModuleTask(frame, task);

            lock (_tasks) _tasks.Add(recordFrameTask);
            await task;
            lock (_tasks) _tasks.Remove(recordFrameTask);
        }

        // TODO: add an output parameter, make this public
        internal async UniTask RecordFrameAsync(long timestamp, int frame)
        {
            var frameDataBuffer = new SerializedSamplesBuffer(Allocator.Persistent);
            await RecordFrameDataAsync(frameDataBuffer);
            // TODO: Pack frame data (convert payload to Any and add header)
            Debug.Log("Pushing frame data to recorder: Frame " + frame + ", " + frameDataBuffer.GetData().Length +
                      " bytes");
            frameDataBuffer.Dispose();
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
            var frameRecorderModuleTasks = new List<FrameDataRecorderModuleTask>();

            foreach (var module in _asyncFrameDataRecorderModules)
            {
                var moduleBuffer = new SerializedSamplesBuffer(Allocator.Persistent);
                // Fire the task but don't wait for it to finish yet. This is to allow all modules to run their
                // synchronous code first (like querying the object states on main thread) before running their
                // async code (eg. serialization).
                var task = module.RecordFrameDataAsync(moduleBuffer);
                frameRecorderModuleTasks.Add(new FrameDataRecorderModuleTask(task, moduleBuffer));
            }

            foreach (var asyncTask in frameRecorderModuleTasks)
            {
                await asyncTask.Task;
                serializedSamplesBuffer.Merge(asyncTask.Buffer);
            }
        }

        /// <summary>
        /// Waits synchronously for all the frame recording tasks to finish. This method is called automatically by <see cref="PlumeRecorder.Stop"/> when the recorder stops.
        /// </summary>
        public void CompleteTasks()
        {
            // Wait for all the frame recording tasks to finish.
            lock (_tasks)
            {
                if (_tasks.Any())
                    Debug.Log("Waiting for " + _tasks.Count + " frame recording tasks to finish");

                foreach (var recordTask in _tasks)
                {
                    // TODO: ensure that this doesn't create a deadlock
                    recordTask.Task.AsTask().Wait();
                }

                _tasks.Clear();
            }
        }
    }
}