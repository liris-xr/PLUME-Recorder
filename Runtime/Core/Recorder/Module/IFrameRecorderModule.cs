namespace PLUME.Core.Recorder.Module
{
    public interface IFrameRecorderModule : IRecorderModule
    {
        internal void RecordFrameData(FrameDataBuffer buffer);
    }
}