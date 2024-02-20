using ProtoBurst;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Collections;

namespace PLUME.Core.Recorder.Module
{
    public interface IFrameDataBatchSerializer<T> where T : unmanaged, IProtoBurstMessage
    {
        public NativeList<byte> SerializeFrameDataBatch(NativeArray<T> samples, SampleTypeUrl sampleTypeUrl, Allocator allocator, int batchSize = 128);
    }
}