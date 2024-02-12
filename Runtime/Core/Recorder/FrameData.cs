using System;
using Unity.Collections;

namespace PLUME.Core.Recorder
{
    public struct FrameData : IDisposable
    {
        internal readonly long Timestamp;
        internal readonly int Frame;
        internal SerializedSamplesBuffer Buffer;

        public FrameData(Allocator allocator, long timestamp, int frame)
        {
            Timestamp = timestamp;
            Frame = frame;
            Buffer = new SerializedSamplesBuffer(allocator);
        }

        public void Dispose()
        {
            Buffer.Dispose();
        }
    }
}