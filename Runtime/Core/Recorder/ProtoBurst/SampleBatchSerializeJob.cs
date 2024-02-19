using ProtoBurst;
using ProtoBurst.Message;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace PLUME.Core.Recorder.ProtoBurst
{
    [BurstCompile]
    public struct SampleBatchSerializeJob<T> : IJobParallelForBatch where T : unmanaged, IProtoBurstMessage
    {
        public int SampleMaxSize;

        [ReadOnly] public NativeArray<T> Samples;

        public SampleTypeUrl SampleTypeUrl;

        public NativeList<byte>.ParallelWriter Bytes;

        public unsafe void Execute(int startIndex, int count)
        {
            var buffer = new BufferWriter(SampleMaxSize * count, Allocator.Temp);

            for (var i = startIndex; i < startIndex + count; i++)
            {
                var sample = Samples[i];
                Any.WriteTo(ref SampleTypeUrl, ref sample, ref buffer);
            }

            var arr = buffer.AsArray();
            Bytes.AddRangeNoResize(arr.GetUnsafeReadOnlyPtr(), arr.Length);
            buffer.Dispose();
        }
    }
}