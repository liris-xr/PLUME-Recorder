using System;

namespace PLUME.Core.Recorder.Module.Frame
{
    public interface IFrameData : IDisposable
    {
        public void Serialize(FrameDataWriter frameDataWriter);
    }
}