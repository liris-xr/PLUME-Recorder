using Cysharp.Threading.Tasks;

namespace PLUME.Core.Recorder.Module
{
    public abstract class ObjectFrameRecorderModuleAsync<TObject> : ObjectRecorderModule<TObject>,
        IFrameRecorderModuleAsync where TObject : UnityEngine.Object
    {
        async UniTask IFrameRecorderModuleAsync.RecordFrameDataAsync(FrameDataBuffer buffer)
        {
            await OnRecordFrameData(buffer);
            ClearCreatedObjects();
            ClearDestroyedObjects();
        }

        protected abstract UniTask OnRecordFrameData(FrameDataBuffer buffer);
    }
}