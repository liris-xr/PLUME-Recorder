using PLUME.Core.Recorder.Data;
using ProtoBurst;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace PLUME.Core.Recorder.ProtoBurst
{
    [BurstCompile]
    public struct SampleBatchPrepareSerializeJob<T> : IJob where T : unmanaged, IProtoBurstMessage
    {
        [ReadOnly] public NativeArray<T> Samples;

        public DataChunks SerializedData;

        [WriteOnly] public NativeArray<int> ChunksByteOffset;

        public void Execute()
        {
            var byteOffset = 0;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < Samples.Length; i++)
            {
                var chunkLength = Samples[i].ComputeSize();
                SerializedData.AddUninitialized(chunkLength);
                ChunksByteOffset[i] = byteOffset;
                byteOffset += chunkLength;
            }
        }
    }
}