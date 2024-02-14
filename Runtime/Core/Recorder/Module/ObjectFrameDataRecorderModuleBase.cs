using System.Threading;
using Cysharp.Threading.Tasks;

namespace PLUME.Core.Recorder.Module
{
    public abstract class ObjectFrameDataRecorderModuleBase<TObject> : ObjectRecorderModuleBase<TObject>,
        IFrameDataRecorderModule where TObject : UnityEngine.Object
    {
        
        async UniTask IFrameDataRecorderModule.RecordFrameData(SerializedSamplesBuffer buffer,
            CancellationToken cancellationToken)
        {
            await OnRecordFrameData(buffer, cancellationToken);
            ClearCreatedObjects();
            ClearDestroyedObjects();
        }

        protected abstract UniTask OnRecordFrameData(SerializedSamplesBuffer buffer,
            CancellationToken forceStopToken);
    }
}