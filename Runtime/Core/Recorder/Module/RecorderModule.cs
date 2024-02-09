namespace PLUME.Core.Recorder.Module
{
    public abstract class RecorderModule : IRecorderModule
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
    }
}