namespace PLUME.Core.Recorder.Module.Frame
{
    public interface IFrameDataRecorderModule : IRecorderModule
    {
        internal void EnqueueFrameData(FrameInfo frameInfo);

        internal bool SerializeFrameData(FrameInfo frameInfo, FrameDataWriter output);

        internal void DisposeFrameData(FrameInfo frameInfo);
    }
}