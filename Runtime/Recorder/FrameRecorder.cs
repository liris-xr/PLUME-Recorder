using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PLUME.Recorder.Module;
using Unity.Collections;

namespace PLUME.Recorder
{
    public class FrameRecorder
    {
        private readonly IUnityFrameRecorderModule[] _modules;
        private readonly IUnityFrameRecorderModuleAsync[] _asyncModules;

        public FrameRecorder(IUnityFrameRecorderModule[] modules, IUnityFrameRecorderModuleAsync[] asyncModules)
        {
            _modules = modules;
            _asyncModules = asyncModules;
        }
        
        public async UniTask<FrameData> RecordFrameAsync(long timestamp, int frame)
        {
            var frameData = new FrameData(Allocator.Persistent, timestamp, frame);
            
            // Run all the synchronous modules
            foreach (var module in _modules)
            {
                var buffer = new FrameDataBuffer(Allocator.Persistent);
                module.RecordFrame(buffer);
                frameData.Buffer.Merge(buffer);
                buffer.Dispose();
            }

            // Run all the asynchronous modules
            var asyncTasks = new List<AsyncTask>();
            
            foreach (var module in _asyncModules)
            {
                var buffer = new FrameDataBuffer(Allocator.Persistent);
                // Fire the task but don't wait for it to finish yet. This is to allow all modules to run their
                // synchronous code first (like querying the object states on main thread) before running their async code (eg. serialization).
                var task = module.RecordFrameAsync(buffer);
                asyncTasks.Add(new AsyncTask(task, buffer));
            }

            foreach (var asyncTask in asyncTasks)
            {
                await asyncTask.Task;
                frameData.Buffer.Merge(asyncTask.Buffer);
            }

            return frameData;
        }

        private readonly struct AsyncTask
        {
            public readonly UniTask Task;
            public readonly FrameDataBuffer Buffer;
            
            public AsyncTask(UniTask task, FrameDataBuffer buffer)
            {
                Task = task;
                Buffer = buffer;
            }
        }
    }
}