using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using PLUME.Core.Recorder.Module;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Core.Utils;
using Unity.Collections;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Scripting;

namespace PLUME.Core.Recorder
{
    [Preserve]
    public class FrameRecorderModule : RecorderModule
    {
        private IFrameDataRecorderModule[] _frameDataRecorderModules;
        private IFrameDataRecorderModuleAsync[] _asyncFrameDataRecorderModules;

        private readonly HashSet<FrameRecorderModuleTask> _tasks = new();

        protected override void OnCreate(PlumeRecorder recorder)
        {
            var recorderModules = recorder.GetRecorderModules();
            _frameDataRecorderModules = recorderModules.OfType<IFrameDataRecorderModule>().ToArray();
            _asyncFrameDataRecorderModules = recorderModules.OfType<IFrameDataRecorderModuleAsync>().ToArray();
            PlayerLoopUtils.InjectUpdateInCurrentLoop(typeof(FrameRecorderModule), Update, typeof(PostLateUpdate));
        }

        private async void Update()
        {
            if (!IsRecording)
                return;

            var timestamp = PlumeRecorder.Instance.Clock.ElapsedNanoseconds;
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

        protected override void OnStop()
        {
            CompleteTasks();
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
    }
}