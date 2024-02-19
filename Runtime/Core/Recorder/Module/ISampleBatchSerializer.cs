using PLUME.Core.Recorder.Data;
using ProtoBurst;
using Unity.Collections;

namespace PLUME.Core.Recorder.Module
{
    public interface ISampleBatchSerializer<T> where T : unmanaged, IProtoBurstMessage
    {
        public DataChunks SerializeBatch(NativeArray<T> samples, Allocator allocator);
    }
}