using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PLUME.Recorder.Module
{
    public abstract class UnityObjectFrameRecorderModuleAsync<TObject> : UnityObjectRecorderModule<TObject>,
        IUnityFrameRecorderModuleAsync where TObject : Object
    {
        async UniTask IUnityFrameRecorderModuleAsync.RecordFrameAsync(FrameDataBuffer buffer)
        {
            await OnRecordFrame(buffer);
            ClearCreatedObjects();
            ClearDestroyedObjects();
        }

        protected abstract UniTask OnRecordFrame(FrameDataBuffer buffer);
    }
}