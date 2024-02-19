using PLUME.Base.Module.Unity.Transform;
using PLUME.Core.Recorder.Module;
using PLUME.Core.Recorder.ProtoBurst;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

// [assembly: RegisterGenericJobType(typeof(SampleBatchSerializeJob<TransformUpdateLocalPositionSample>))]
// [assembly: RegisterGenericJobType(typeof(SampleBatchComputeTotalSizeJob<TransformUpdateLocalPositionSample>))]

namespace PLUME.Base.Module.Unity.Transform
{
    [BurstCompile]
    public struct TransformSampleBatchSerializer : ISampleBatchSerializer<TransformUpdateLocalPositionSample>
    {
        private SampleBatchSerializeJob<TransformUpdateLocalPositionSample> _serializeJob;
        private SampleBatchComputeMaxSizeJob _computeMaxSizeJob;
        private SampleBatchComputeTotalSizeJob<TransformUpdateLocalPositionSample> _computeTotalSizeJob;
        
        public NativeList<byte> SerializeBatch(NativeList<TransformUpdateLocalPositionSample> samples, Allocator allocator)
        {
            var sampleTypeUrl = samples[0].GetTypeUrl(Allocator.Persistent);

            var size = new NativeArray<int>(1, Allocator.Persistent);
            var maxSizes = new NativeQueue<int>(Allocator.Persistent);
            var maxSize = new NativeArray<int>(1, Allocator.Persistent);

            _computeTotalSizeJob.TotalSize = size;
            _computeTotalSizeJob.MaxSizes = maxSizes;
            _computeTotalSizeJob.Samples = samples.AsArray();
            _computeTotalSizeJob.SampleTypeUrl = sampleTypeUrl;
            _computeTotalSizeJob.Run(samples.Length, 128);

            _computeMaxSizeJob.MaxSizes = maxSizes;
            _computeMaxSizeJob.MaxSize = maxSize;
            _computeMaxSizeJob.Run();

            var totalSize = size[0];
            var sampleMaxSize = maxSize[0];

            size.Dispose();
            maxSizes.Dispose();
            maxSize.Dispose();

            var bytes = new NativeList<byte>(totalSize, allocator);
            _serializeJob.SampleMaxSize = sampleMaxSize;
            _serializeJob.Samples = samples.AsArray();
            _serializeJob.Bytes = bytes.AsParallelWriter();
            _serializeJob.SampleTypeUrl = sampleTypeUrl;
            _serializeJob.Run(samples.Length, 128);
            sampleTypeUrl.Dispose();

            return bytes;
        }
    }
}