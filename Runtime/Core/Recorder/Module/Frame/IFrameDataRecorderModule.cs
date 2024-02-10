namespace PLUME.Core.Recorder.Module.Frame
{
    public interface IFrameDataRecorderModule : IRecorderModule
    {
        internal void RecordFrameData(SerializedSamplesBuffer buffer);
    }
}