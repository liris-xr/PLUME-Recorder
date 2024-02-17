using PLUME.Core.Recorder.Data;

namespace PLUME.Core.Recorder.Module
{
    public interface IFrameDataRecorderModule : IRecorderModule
    {
        internal void CollectFrameData(Frame frame);

        internal bool SerializeFrameData(Frame frame, FrameDataChunks output);
        
        internal void DisposeFrameData(Frame frame);
    }
}