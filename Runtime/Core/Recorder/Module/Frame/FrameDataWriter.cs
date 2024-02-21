using ProtoBurst;
using Unity.Collections;
using Unity.Jobs;

namespace PLUME.Core.Recorder.Module.Frame
{
    public struct FrameDataWriter
    {
        private NativeList<byte> _frameDataRawBytes;

        public FrameDataWriter(NativeList<byte> frameDataRawBytes)
        {
            _frameDataRawBytes = frameDataRawBytes;
        }

        public void WriteBatch<TU>(NativeArray<TU> samples,
            FrameDataBatchPrepareSerializeJob<TU> prepareBatchJob,
            FrameDataBatchSerializeJob<TU> serializeBatchJob, int batchSize = 128)
            where TU : unmanaged, IProtoBurstMessage
        {
            if (samples.Length == 0)
                return;

            var sampleTypeUrl = samples[0].GetTypeUrl(Allocator.Persistent);

            var totalSize = new NativeArray<int>(1, Allocator.Persistent);
            var serializedSampleSizes = new NativeArray<int>(samples.Length, Allocator.Persistent);
            var sampleSizes = new NativeArray<int>(samples.Length, Allocator.Persistent);

            totalSize[0] = 0;

            prepareBatchJob.TotalSize = totalSize;
            prepareBatchJob.Samples = samples;
            prepareBatchJob.SampleTypeUrlBytes = sampleTypeUrl.AsArray();
            prepareBatchJob.SampleSizes = sampleSizes;
            prepareBatchJob.SerializedSampleSizes = serializedSampleSizes;
            prepareBatchJob.Run(samples.Length, batchSize);

            var frameDataRawBytes = new NativeList<byte>(totalSize[0], Allocator.Persistent);
            totalSize.Dispose();

            serializeBatchJob.SerializedData = frameDataRawBytes.AsParallelWriter();
            serializeBatchJob.Samples = samples;
            serializeBatchJob.SampleTypeUrlBytes = sampleTypeUrl.AsArray();
            serializeBatchJob.SampleSizes = sampleSizes;
            serializeBatchJob.SerializedSampleSizes = serializedSampleSizes;
            serializeBatchJob.Run(samples.Length, batchSize);
            serializedSampleSizes.Dispose();
            sampleSizes.Dispose();

            WriteRaw(frameDataRawBytes);
            
            sampleTypeUrl.Dispose();
            frameDataRawBytes.Dispose();
        }

        public void WriteRaw(NativeList<byte> frameDataRawBytes)
        {
            _frameDataRawBytes.AddRange(frameDataRawBytes.AsArray());
        }
    }
}