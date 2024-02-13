using Cysharp.Threading.Tasks;
using PLUME.Core.Object.SafeRef;

namespace PLUME.Core.Recorder.Module.Frame
{
    public abstract class FrameDataRecorderModuleAsync : IFrameDataRecorderModuleAsync
    {
        void IRecorderModule.Create(ObjectSafeRefProvider objSafeRefProvider, SampleTypeUrlRegistry typeUrlRegistry)
        {
            OnCreate(objSafeRefProvider, typeUrlRegistry);
        }

        void IRecorderModule.Destroy()
        {
            OnDestroy();
        }

        void IRecorderModule.Start()
        {
            OnStart();
        }

        void IRecorderModule.Stop()
        {
            OnStop();
        }

        void IRecorderModule.Reset()
        {
            OnReset();
        }

        UniTask IFrameDataRecorderModuleAsync.RecordFrameDataAsync(SerializedSamplesBuffer buffer)
        {
            return OnRecordFrameData(buffer);
        }

        protected virtual void OnCreate(ObjectSafeRefProvider objSafeRefProvider, SampleTypeUrlRegistry typeUrlRegistry)
        {
        }

        protected virtual void OnDestroy()
        {
        }

        protected virtual void OnStart()
        {
        }

        protected virtual void OnStop()
        {
        }

        protected virtual void OnReset()
        {
        }

        protected virtual UniTask OnRecordFrameData(SerializedSamplesBuffer buffer)
        {
            return UniTask.CompletedTask;
        }
    }
}