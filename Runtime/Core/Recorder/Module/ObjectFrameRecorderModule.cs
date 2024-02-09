namespace PLUME.Core.Recorder.Module
{
    public abstract class ObjectFrameRecorderModule<TObject> : ObjectRecorderModule<TObject>,
        IFrameRecorderModule where TObject : UnityEngine.Object
    {
        void IFrameRecorderModule.RecordFrameData(FrameDataBuffer buffer)
        {
            OnRecordFrameData(buffer);
            ClearCreatedObjects();
            ClearDestroyedObjects();
        }
        
        protected abstract void OnRecordFrameData(FrameDataBuffer buffer);
    }
}