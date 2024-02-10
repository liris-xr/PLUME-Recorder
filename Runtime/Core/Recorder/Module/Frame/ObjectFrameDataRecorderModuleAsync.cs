using Cysharp.Threading.Tasks;

namespace PLUME.Core.Recorder.Module.Frame
{
    public abstract class ObjectFrameDataRecorderModuleAsync<TObject> : ObjectRecorderModule<TObject>,
        IFrameDataRecorderModuleAsync where TObject : UnityEngine.Object
    {
        async UniTask IFrameDataRecorderModuleAsync.RecordFrameDataAsync(SerializedSamplesBuffer buffer)
        {
            await OnRecordFrameData(buffer);
            ClearCreatedObjects();
            ClearDestroyedObjects();
        }

        protected abstract UniTask OnRecordFrameData(SerializedSamplesBuffer buffer);
    }
}