using ProtoBurst;
using Unity.Collections;

namespace PLUME.Core.Recorder.Module.Frame
{
    public interface IFrameDataBatchSerializer<TU> where TU : unmanaged, IProtoBurstMessage
    {
        public NativeList<byte> SerializeFrameDataBatch(NativeArray<TU> samples, Allocator allocator,
            int batchSize = 128);
    }
}