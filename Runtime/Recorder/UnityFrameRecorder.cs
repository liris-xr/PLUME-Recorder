using System;
using PLUME.Recorder.Module;

namespace PLUME.Recorder
{
    public class UnityFrameRecorder
    {
        private readonly IUnityRecorderModule[] _modules;

        public UnityFrameRecorder(IUnityRecorderModule[] modules)
        {
            _modules = modules;
        }

        internal void Start()
        {
            Array.ForEach(_modules, m => m.Start());
        }

        public void Stop()
        {
            Array.ForEach(_modules, m => m.Stop());
        }

        internal FrameData RecordFrame(long timestamp)
        {
            // TODO: Use a pool for frameData
            var frameData = new FrameData(timestamp);

            foreach (var module in _modules)
            {
                module.RecordFrame(frameData);
            }

            return frameData;
        }
    }
}