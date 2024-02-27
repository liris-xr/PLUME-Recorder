namespace PLUME.Core.Recorder.Module
{
    public interface IRecorderModule
    {
        internal void Create(RecorderContext ctx);

        internal void Destroy(RecorderContext ctx);

        internal void Awake(RecorderContext ctx);

        internal void StartRecording(RecorderContext ctx);

        internal void StopRecording(RecorderContext ctx);
    }
}