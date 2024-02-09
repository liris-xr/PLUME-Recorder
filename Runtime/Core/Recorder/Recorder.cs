using System.Linq;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder.Module;
using UnityObject = UnityEngine.Object;

namespace PLUME.Core.Recorder
{
    public class Recorder
    {
        private bool _isRecording;

        private readonly Clock _clock;
        private readonly FrameRecorder _frameRecorder;
        private readonly IRecorderModule[] _recorderModules;

        public Recorder(IRecorderModule[] recorderModules)
        {
            _clock = new Clock();
            _recorderModules = recorderModules;
            var frameRecorderModules = recorderModules.OfType<IFrameRecorderModule>().ToArray();
            var asyncFrameRecorderModules = recorderModules.OfType<IFrameRecorderModuleAsync>().ToArray();
            _frameRecorder = new FrameRecorder(_clock, frameRecorderModules, asyncFrameRecorderModules);
        }

        internal void Initialize()
        {
            _frameRecorder.Initialize();
        }
        
        public void Start()
        {
            if (_isRecording)
                throw new System.InvalidOperationException("Recorder is already recording");
            
            _clock.Reset();
            foreach (var module in _recorderModules)
                module.Start();
            
            _frameRecorder.Start();
            _clock.Start();
            _isRecording = true;
        }

        public void Stop()
        {
            if (!_isRecording)
                throw new System.InvalidOperationException("Recorder is not recording");
            
            _clock.Stop();

            foreach (var module in _recorderModules)
                module.Stop();

            _frameRecorder.Stop();
            _isRecording = false;
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