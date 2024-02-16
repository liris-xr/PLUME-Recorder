namespace PLUME.Core.Recorder.Module
{
    public interface IFrameDataRecorderModule : IRecorderModule
    {
        internal void PushFrameData(Frame frame);

        internal bool TryPopSerializedFrameData(Frame frame, SerializedSamplesBuffer buffer);
    }
}