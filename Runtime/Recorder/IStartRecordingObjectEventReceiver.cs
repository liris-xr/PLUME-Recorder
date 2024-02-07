using UnityEngine;

namespace PLUME.Recorder
{
    public interface IStartRecordingObjectEventReceiver
    {
        public void OnStartRecordingObject(Object obj);
        
        public int ExecutionPriority()
        {
            return 1000;
        }
    }
}