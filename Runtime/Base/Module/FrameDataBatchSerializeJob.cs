using PLUME.Sample.ProtoBurst;
using ProtoBurst;
using ProtoBurst.Message;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace PLUME.Base.Module
{
    [BurstCompile]
    public struct FrameDataBatchSerializeJob<T> : IJobParallelForBatch where T : unmanaged, IProtoBurstMessage
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<T> Samples;
        
        [ReadOnly] public NativeArray<byte> SampleTypeUrlBytes;
        
        [ReadOnly] public NativeArray<int> SampleSizes;
        
        [ReadOnly] public NativeArray<int> SerializedSampleSizes;

        [WriteOnly] public NativeList<byte>.ParallelWriter SerializedData;

        public void Execute(int startIndex, int count)
        {
            var batchBytes = new NativeList<byte>(Allocator.Temp);
            var bufferWriter = new BufferWriter(batchBytes);
            
            for (var i = startIndex; i < startIndex + count; i++)
            {
                var sample = Samples[i];
                var sampleSize = SampleSizes[i];
                var serializedSampleSize = SerializedSampleSizes[i];
                
                batchBytes.Capacity = batchBytes.Length + serializedSampleSize;
                
                bufferWriter.WriteTag(Frame.DataFieldTag);
                bufferWriter.WriteLength(Any.ComputeSize(SampleTypeUrlBytes.Length, sampleSize));
                Any.WriteTo(ref SampleTypeUrlBytes, ref sample, ref bufferWriter);
            }
            
            SerializedData.AddRangeNoResize(batchBytes);
            batchBytes.Dispose();
        }
    }
}