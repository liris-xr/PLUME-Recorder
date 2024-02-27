using PLUME.Core.Recorder.Module.Frame;
using ProtoBurst;
using Unity.Collections;
using Unity.Jobs;

namespace PLUME.Base.Module.Unity
{
    public struct FrameDataBatchSerializer<TU> : IFrameDataBatchSerializer<TU> where TU : unmanaged, IProtoBurstMessage
    {
        private FrameDataBatchPrepareSerializeJob<TU> _prepareBatchJob;
        private FrameDataBatchSerializeJob<TU> _serializeBatchJob;

        public FrameDataBatchSerializer(
            FrameDataBatchPrepareSerializeJob<TU> prepareBatchJob,
            FrameDataBatchSerializeJob<TU> serializeBatchJob)
        {
            _prepareBatchJob = prepareBatchJob;
            _serializeBatchJob = serializeBatchJob;
        }
        
        public NativeList<byte> SerializeFrameDataBatch(NativeArray<TU> samples, Allocator allocator, int batchSize = 128)
        {
            var frameDataRawBytes = new NativeList<byte>(allocator);
            
            if (samples.Length == 0)
                return frameDataRawBytes;

            var sampleTypeUrl = samples[0].GetTypeUrl(allocator);

            var totalSize = new NativeArray<int>(1, allocator);
            var serializedSampleSizes = new NativeArray<int>(samples.Length, allocator);
            var sampleSizes = new NativeArray<int>(samples.Length, allocator);

            totalSize[0] = 0;

            _prepareBatchJob.TotalSize = totalSize;
            _prepareBatchJob.Samples = samples;
            _prepareBatchJob.SampleTypeUrlBytes = sampleTypeUrl.AsArray();
            _prepareBatchJob.SampleSizes = sampleSizes;
            _prepareBatchJob.SerializedSampleSizes = serializedSampleSizes;
            _prepareBatchJob.Run(samples.Length, batchSize);

            frameDataRawBytes.SetCapacity(totalSize[0]);
            totalSize.Dispose();

            _serializeBatchJob.SerializedData = frameDataRawBytes.AsParallelWriter();
            _serializeBatchJob.Samples = samples;
            _serializeBatchJob.SampleTypeUrlBytes = sampleTypeUrl.AsArray();
            _serializeBatchJob.SampleSizes = sampleSizes;
            _serializeBatchJob.SerializedSampleSizes = serializedSampleSizes;
            _serializeBatchJob.Run(samples.Length, batchSize);
            
            serializedSampleSizes.Dispose();
            sampleSizes.Dispose();
            sampleTypeUrl.Dispose();
            
            return frameDataRawBytes;
        }
    }
}