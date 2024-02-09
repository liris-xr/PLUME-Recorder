using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using PLUME.Core.Recorder.Module;
using PLUME.Core.Utils;
using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityObject = UnityEngine.Object;

namespace PLUME.Core.Recorder
{
    public class FrameRecorder
    {
        private readonly IFrameRecorderModule[] _frameRecorderModules;
        private readonly IFrameRecorderModuleAsync[] _asyncFrameRecorderModules;

        private readonly HashSet<FrameRecorderTask> _tasks = new();

        public FrameRecorder(IFrameRecorderModule[] frameRecorderModules,
            IFrameRecorderModuleAsync[] asyncFrameRecorderModules)
        {
            _frameRecorderModules = frameRecorderModules;
            _asyncFrameRecorderModules = asyncFrameRecorderModules;
        }

        public void Initialize()
        {
            InjectUpdateLoop();
        }

        public void CompleteTasks()
        {
            // Wait for all the frame recording tasks to finish.
            lock (_tasks)
            {
                if (_tasks.Any())
                    Debug.Log("Waiting for " + _tasks.Count + " frame recording tasks to finish");

                foreach (var recordTask in _tasks)
                {
                    recordTask.Task.AsTask().Wait();
                }

                _tasks.Clear();
            }
        }

        private void InjectUpdateLoop()
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            var updateType = typeof(Recorder);
            var playerLoopSystemType = typeof(PlayerLoopSystem);
            PlayerLoopUtils.AppendToPlayerLoopList(updateType, Update, ref playerLoop, playerLoopSystemType);
            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        internal async void Update()
        {
            var timestamp = 0L;
            var frame = 0;
            var task = RecordFrameAsync(timestamp, frame);
            var recordFrameTask = new FrameRecorderTask(frame, task);

            lock (_tasks) _tasks.Add(recordFrameTask);
            await task;
            lock (_tasks) _tasks.Remove(recordFrameTask);
        }

        // TODO: add an output parameter, make this public
        internal async UniTask RecordFrameAsync(long timestamp, int frame)
        {
            var frameDataBuffer = new FrameDataBuffer(Allocator.Persistent);
            await RecordFrameDataAsync(frameDataBuffer);
            // TODO: Pack frame data (convert payload to Any and add header)
            Debug.Log("Pushing frame data to recorder: Frame " + frame + ", " + frameDataBuffer.Data.Length + " bytes");
            frameDataBuffer.Dispose();
        }

        internal async UniTask RecordFrameDataAsync(FrameDataBuffer frameDataBuffer)
        {
            // Run all the synchronous modules
            foreach (var module in _frameRecorderModules)
            {
                var moduleBuffer = new FrameDataBuffer(Allocator.Persistent);
                module.RecordFrameData(moduleBuffer);
                frameDataBuffer.Merge(moduleBuffer);
                moduleBuffer.Dispose();
            }

            // Run all the asynchronous modules
            var frameRecorderModuleTasks = new List<FrameRecorderModuleTask>();

            foreach (var module in _asyncFrameRecorderModules)
            {
                var moduleBuffer = new FrameDataBuffer(Allocator.Persistent);
                // Fire the task but don't wait for it to finish yet. This is to allow all modules to run their
                // synchronous code first (like querying the object states on main thread) before running their
                // async code (eg. serialization).
                var task = module.RecordFrameDataAsync(moduleBuffer);
                frameRecorderModuleTasks.Add(new FrameRecorderModuleTask(task, moduleBuffer));
            }

            foreach (var asyncTask in frameRecorderModuleTasks)
            {
                await asyncTask.Task;
                frameDataBuffer.Merge(asyncTask.Buffer);
            }
        }
    }
}