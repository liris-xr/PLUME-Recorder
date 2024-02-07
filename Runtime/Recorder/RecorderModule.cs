using UnityEngine;

namespace PLUME.Recorder
{
    public abstract class RecorderModule : MonoBehaviour, IStartRecordingEventReceiver
    {
        public Recorder recorder;
        
        public void OnStartRecording()
        {
            ResetCache();
        }

        protected abstract void ResetCache();
    }
}