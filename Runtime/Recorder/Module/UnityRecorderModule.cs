namespace PLUME.Recorder.Module
{
    public abstract class UnityRecorderModule : IUnityRecorderModule
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

        void IUnityRecorderModule.RecordFrame(FrameData frameData)
        {
            OnRecordFrame(frameData);
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
        
        protected virtual void OnRecordFrame(FrameData frameData)
        {
        }
    }
}