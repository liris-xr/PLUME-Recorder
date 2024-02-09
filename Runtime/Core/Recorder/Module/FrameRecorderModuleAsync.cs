using Cysharp.Threading.Tasks;

namespace PLUME.Core.Recorder.Module
{
    public abstract class FrameRecorderModuleAsync : IFrameRecorderModuleAsync
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

        UniTask IFrameRecorderModuleAsync.RecordFrameDataAsync(FrameDataBuffer buffer)
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

        protected virtual UniTask OnRecordFrameData(FrameDataBuffer buffer)
        {
            return UniTask.CompletedTask;
        }
    }
}