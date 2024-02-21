using System.Threading;
using PLUME.Sample.ProtoBurst;
using ProtoBurst;
using ProtoBurst.Message;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace PLUME.Core.Recorder.Module.Frame
{
    [BurstCompile]
    public struct FrameDataBatchPrepareSerializeJob<T> : IJobParallelForBatch where T : unmanaged, IProtoBurstMessage
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<T> Samples;

        [ReadOnly] public NativeArray<byte> SampleTypeUrlBytes;
        
        [NativeDisableParallelForRestriction]
        public NativeArray<int> TotalSize;
        
        [WriteOnly] public NativeArray<int> SampleSizes;
        
        [WriteOnly] public NativeArray<int> SerializedSampleSizes;

        public unsafe void Execute(int startIndex, int count)
        {
            var size = 0;

            for (var i = startIndex; i < startIndex + count; i++)
            {
                var sample = Samples[i];
                var sampleSize = sample.ComputeSize();
                var packedSampleSize = Any.ComputeSize(SampleTypeUrlBytes.Length, sampleSize);
                var serializedSampleSize = BufferExtensions.ComputeTagSize(FrameSample.DataFieldTag) +
                                           BufferExtensions.ComputeLengthPrefixSize(packedSampleSize) +
                                           packedSampleSize;
                
                SampleSizes[i] = sampleSize;
                SerializedSampleSizes[i] = serializedSampleSize;
                size += serializedSampleSize;
            }

            Interlocked.Add(ref ((int*)TotalSize.GetUnsafePtr())[0], size);
        }
    }
}