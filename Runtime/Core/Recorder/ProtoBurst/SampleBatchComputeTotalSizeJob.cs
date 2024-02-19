using System.Threading;
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
    public struct SampleBatchComputeTotalSizeJob<T> : IJobParallelForBatch where T : unmanaged, IProtoBurstMessage
    {
        public NativeArray<int> TotalSize;

        public NativeQueue<int> MaxSizes;

        [ReadOnly] public NativeArray<T> Samples;

        public SampleTypeUrl SampleTypeUrl;

        public unsafe void Execute(int startIndex, int count)
        {
            var maxSize = 0;

            for (var i = startIndex; i < startIndex + count; i++)
            {
                var size = Any.ComputeSize(SampleTypeUrl.BytesLength, Samples[i].ComputeSize());
                Interlocked.Add(ref ((int*)TotalSize.GetUnsafePtr())[0], size);

                if (size > maxSize)
                    maxSize = size;
            }

            MaxSizes.Enqueue(maxSize);
        }
    }
}