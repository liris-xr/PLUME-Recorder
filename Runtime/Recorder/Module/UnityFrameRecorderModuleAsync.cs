using Cysharp.Threading.Tasks;

namespace PLUME.Recorder.Module
{
    public abstract class UnityFrameRecorderModuleAsync : IUnityFrameRecorderModuleAsync
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

        UniTask IUnityFrameRecorderModuleAsync.RecordFrameAsync(FrameDataBuffer buffer)
        {
            return OnRecordFrame(buffer);
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

        protected virtual UniTask OnRecordFrame(FrameDataBuffer buffer)
        {
            return UniTask.CompletedTask;
        }
    }
}