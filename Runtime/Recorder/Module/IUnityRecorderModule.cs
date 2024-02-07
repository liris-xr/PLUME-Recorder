namespace PLUME.Recorder.Module
{
    public interface IUnityRecorderModule : IRecorderModule
    {
        internal void RecordFrame(FrameData frameData);
    }
}