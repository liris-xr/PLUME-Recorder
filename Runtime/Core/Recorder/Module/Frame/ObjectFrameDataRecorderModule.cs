namespace PLUME.Core.Recorder.Module.Frame
{
    public abstract class ObjectFrameDataRecorderModule<TObject> : ObjectRecorderModule<TObject>,
        IFrameDataRecorderModule where TObject : UnityEngine.Object
    {
        void IFrameDataRecorderModule.RecordFrameData(SerializedSamplesBuffer buffer)
        {
            OnRecordFrameData(buffer);
            ClearCreatedObjects();
            ClearDestroyedObjects();
        }
        
        protected abstract void OnRecordFrameData(SerializedSamplesBuffer buffer);
    }
}