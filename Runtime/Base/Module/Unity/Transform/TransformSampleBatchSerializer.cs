using PLUME.Core.Recorder.Module;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace PLUME.Base.Module.Unity.Transform
{
    [BurstCompile]
    public struct TransformFrameDataBatchSerializer : IFrameDataBatchSerializer<TransformUpdateLocalPositionSample>
    {
        private FrameDataBatchPrepareSerializeJob<TransformUpdateLocalPositionSample> _prepareSerializeJob;
        private FrameDataBatchSerializeJob<TransformUpdateLocalPositionSample> _serializeJob;

        public NativeList<byte> SerializeFrameDataBatch(NativeArray<TransformUpdateLocalPositionSample> samples,
            SampleTypeUrl sampleTypeUrl, Allocator allocator, int batchSize)
        {
            var totalSize = new NativeArray<int>(1, Allocator.Persistent);
            var serializedSampleSizes = new NativeArray<int>(samples.Length, Allocator.Persistent);
            var sampleSizes = new NativeArray<int>(samples.Length, Allocator.Persistent);

            totalSize[0] = 0;
            
            _prepareSerializeJob.TotalSize = totalSize;
            _prepareSerializeJob.Samples = samples;
            _prepareSerializeJob.SampleTypeUrlBytes = sampleTypeUrl.AsArray();
            _prepareSerializeJob.SampleSizes = sampleSizes;
            _prepareSerializeJob.SerializedSampleSizes = serializedSampleSizes;
            _prepareSerializeJob.Run(samples.Length, batchSize);

            var serializedData = new NativeList<byte>(totalSize[0], allocator);
            totalSize.Dispose();

            _serializeJob.SerializedData = serializedData.AsParallelWriter();
            _serializeJob.Samples = samples;
            _serializeJob.SampleTypeUrlBytes = sampleTypeUrl.AsArray();
            _serializeJob.SampleSizes = sampleSizes;
            _serializeJob.SerializedSampleSizes = serializedSampleSizes;
            _serializeJob.Run(samples.Length, batchSize);
            serializedSampleSizes.Dispose();
            sampleSizes.Dispose();

            return serializedData;
        }
    }
}