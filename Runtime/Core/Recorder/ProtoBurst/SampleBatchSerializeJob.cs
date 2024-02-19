using System;
using PLUME.Core.Recorder.Data;
using ProtoBurst;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace PLUME.Core.Recorder.ProtoBurst
{
    [BurstCompile]
    public struct SampleBatchSerializeJob<T> : IJobParallelForBatch where T : unmanaged, IProtoBurstMessage
    {
        [ReadOnly] public NativeArray<T> Samples;

        [NativeDisableParallelForRestriction] public DataChunks SerializedData;

        [ReadOnly] public NativeArray<int> ChunksByteOffset;

        private static void SetBytes(ref DataChunks dataChunks, int byteOffset, ReadOnlySpan<byte> bytes)
        {
            var dst = dataChunks.GetDataSpan().Slice(byteOffset, bytes.Length);
            bytes.CopyTo(dst);
        }

        public void Execute(int startIndex, int count)
        {
            for (var i = startIndex; i < startIndex + count; i++)
            {
                var sampleLength = SerializedData.GetLength(i);
                var sampleBytes = new NativeList<byte>(sampleLength, Allocator.Temp);
                var buffer = new BufferWriter(sampleBytes);
                var sample = Samples[i];
                var sampleByteOffset = ChunksByteOffset[i];
                sample.WriteTo(ref buffer);
                SetBytes(ref SerializedData, sampleByteOffset, sampleBytes.AsArray().AsReadOnlySpan());
                sampleBytes.Dispose();
            }
        }
    }
}