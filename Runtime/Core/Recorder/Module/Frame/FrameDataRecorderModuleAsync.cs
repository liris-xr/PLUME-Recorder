using Cysharp.Threading.Tasks;

namespace PLUME.Core.Recorder.Module.Frame
{
    public abstract class FrameDataRecorderModuleAsync : IFrameDataRecorderModuleAsync
    {
        void IRecorderModule.Create()
        {
            OnCreate();
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

        protected virtual void OnCreate()
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