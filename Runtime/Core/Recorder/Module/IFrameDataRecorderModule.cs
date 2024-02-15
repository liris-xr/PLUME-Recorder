namespace PLUME.Core.Recorder.Module
{
    public interface IFrameDataRecorderModule : IRecorderModule
    {
        internal void EnqueueFrameData();

        internal void DequeueSerializedFrameData(SerializedSamplesBuffer buffer);
    }
}