using UnityEngine;

namespace PLUME.Recorder.Module
{
    public abstract class UnityObjectFrameRecorderModule<TObject> : UnityObjectRecorderModule<TObject>,
        IUnityFrameRecorderModule where TObject : Object
    {
        void IUnityFrameRecorderModule.RecordFrame(FrameDataBuffer buffer)
        {
            OnRecordFrame(buffer);
            ClearCreatedObjects();
            ClearDestroyedObjects();
        }
        
        protected abstract void OnRecordFrame(FrameDataBuffer buffer);
    }
}