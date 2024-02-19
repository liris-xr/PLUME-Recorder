using ProtoBurst;
using Unity.Collections;

namespace PLUME.Core.Recorder.Module
{
    public interface ISampleBatchSerializer<T> where T : unmanaged, IProtoBurstMessage
    {
        public NativeList<byte> SerializeBatch(NativeList<T> samples, Allocator allocator);
    }
}