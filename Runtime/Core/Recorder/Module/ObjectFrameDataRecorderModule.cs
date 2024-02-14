using Cysharp.Threading.Tasks;

namespace PLUME.Core.Recorder.Module
{
    public abstract class ObjectFrameDataRecorderModuleBaseBase<TObject> : ObjectRecorderModuleBase<TObject>, IFrameDataRecorderModule where TObject : UnityEngine.Object
    {
        async UniTask IFrameDataRecorderModule.RecordFrameData(SerializedSamplesBuffer buffer)
        {
            await OnRecordFrameData(buffer);
            ClearCreatedObjects();
            ClearDestroyedObjects();
        }
        
        protected abstract UniTask OnRecordFrameData(SerializedSamplesBuffer buffer);
    }
}