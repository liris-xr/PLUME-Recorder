namespace PLUME.Recorder.Module
{
    public abstract class UnityFrameRecorderModule : IUnityFrameRecorderModule
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

        void IUnityFrameRecorderModule.RecordFrame(FrameDataBuffer buffer)
        {
            OnRecordFrame(buffer);
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

        protected virtual void OnRecordFrame(FrameDataBuffer buffer)
        {
        }
    }
}