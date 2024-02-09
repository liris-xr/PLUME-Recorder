using System;
using Unity.Collections;

namespace PLUME.Recorder
{
    public struct FrameData : IDisposable
    {
        internal readonly long Timestamp;
        internal readonly int Frame;
        internal FrameDataBuffer Buffer;

        public FrameData(Allocator allocator, long timestamp, int frame)
        {
            Timestamp = timestamp;
            Frame = frame;
            Buffer = new FrameDataBuffer(allocator);
        }

        public void Dispose()
        {
            Buffer.Dispose();
        }
    }
}