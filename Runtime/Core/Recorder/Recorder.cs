using System.Linq;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder.Module;
using UnityObject = UnityEngine.Object;

namespace PLUME.Core.Recorder
{
    public class Recorder
    {
        private bool _isRecording;

        private readonly FrameRecorder _frameRecorder;
        private readonly IRecorderModule[] _recorderModules;

        public Recorder(IRecorderModule[] recorderModules)
        {
            _recorderModules = recorderModules;
            var frameRecorderModules = recorderModules.OfType<IFrameRecorderModule>().ToArray();
            var asyncFrameRecorderModules = recorderModules.OfType<IFrameRecorderModuleAsync>().ToArray();
            _frameRecorder = new FrameRecorder(frameRecorderModules, asyncFrameRecorderModules);
        }

        public void Start()
        {
            foreach (var module in _recorderModules)
                module.Start();

            _isRecording = true;
        }

        public void Stop()
        {
            _isRecording = false;

            foreach (var module in _recorderModules)
                module.Stop();

            _frameRecorder.CompleteTasks();
        }

        public void Initialize()
        {
            _frameRecorder.Initialize();
        }

        public bool TryStartRecordingObject<T>(ObjectSafeRef<T> objectSafeRef, bool markCreated) where T : UnityObject
        {
            var started = false;

            foreach (var module in _recorderModules)
            {
                if (module is IObjectRecorderModule objectRecorderModule)
                {
                    started |= objectRecorderModule.TryStartRecordingObject(objectSafeRef, markCreated);
                }
            }

            return started;
        }

        public bool TryStopRecordingObject<T>(ObjectSafeRef<T> objectSafeRef) where T : UnityObject
        {
            var stopped = false;

            foreach (var module in _recorderModules)
            {
                if (module is IObjectRecorderModule objectRecorderModule)
                {
                    stopped |= objectRecorderModule.TryStopRecordingObject(objectSafeRef);
                }
            }

            return stopped;
        }
    }
}