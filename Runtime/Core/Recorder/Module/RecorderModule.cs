using System;

namespace PLUME.Core.Recorder.Module
{
    public abstract class RecorderModule : IRecorderModule
    {
        public bool IsRecording { get; private set; }

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
            IsRecording = true;
            OnStart();
        }

        void IRecorderModule.Stop()
        {
            OnStop();
            IsRecording = false;
        }

        void IRecorderModule.Reset()
        {
            OnReset();
        }

        protected void EnsureIsRecording()
        {
            if (!IsRecording)
            {
                throw new InvalidOperationException("Recorder module is not recording.");
            }
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