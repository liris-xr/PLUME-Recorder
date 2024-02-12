namespace PLUME.Core.Recorder.Module.Frame
{
    public abstract class FrameDataRecorderModule : IFrameDataRecorderModule
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

        void IFrameDataRecorderModule.RecordFrameData(SerializedSamplesBuffer buffer)
        {
            OnRecordFrameData(buffer);
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

        protected virtual void OnRecordFrameData(SerializedSamplesBuffer buffer)
        {
        }
    }
}