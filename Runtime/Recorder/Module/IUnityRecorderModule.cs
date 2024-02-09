namespace PLUME.Recorder.Module
{
    public interface IUnityFrameRecorderModule : IRecorderModule
    {
        internal void RecordFrame(FrameDataBuffer buffer);
    }
}