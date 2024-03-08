namespace PLUME.Core.Recorder.Module
{
    public abstract class RecorderModule : IRecorderModule
    {
        void IRecorderModule.Create(RecorderContext ctx)
        {
            OnCreate(ctx);
        }

        void IRecorderModule.Destroy(RecorderContext ctx)
        {
            OnDestroy(ctx);
        }

        void IRecorderModule.Awake(RecorderContext ctx)
        {
            OnAwake(ctx);
        }

        void IRecorderModule.StartRecording(RecorderContext ctx)
        {
            OnStartRecording(ctx);
        }

        void IRecorderModule.StopRecording(RecorderContext ctx)
        {
            OnStopRecording(ctx);
        }

        protected virtual void OnAwake(RecorderContext ctx)
        {
        }

        protected virtual void OnCreate(RecorderContext ctx)
        {
        }

        protected virtual void OnDestroy(RecorderContext ctx)
        {
        }

        protected virtual void OnStartRecording(RecorderContext ctx)
        {
        }

        protected virtual void OnStopRecording(RecorderContext ctx)
        {
        }
    }
}