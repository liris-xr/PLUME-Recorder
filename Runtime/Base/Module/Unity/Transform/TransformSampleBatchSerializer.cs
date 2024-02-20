using PLUME.Core.Recorder.Data;
using PLUME.Core.Recorder.Module;
using PLUME.Core.Recorder.ProtoBurst;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace PLUME.Base.Module.Unity.Transform
{
    [BurstCompile]
    public struct TransformSampleBatchSerializer : ISampleBatchSerializer<TransformUpdateLocalPositionSample>
    {
        private SampleBatchPrepareSerializeJob<TransformUpdateLocalPositionSample> _prepareSerializeJob;
        private SampleBatchSerializeJob<TransformUpdateLocalPositionSample> _serializeJob;
        
        public DataChunks SerializeBatch(NativeArray<TransformUpdateLocalPositionSample> samples, Allocator allocator)
        {
            var dataChunks = new DataChunks(allocator);
            var chunksByteOffset = new NativeArray<int>(samples.Length, Allocator.Persistent);
            
            _prepareSerializeJob.SerializedData = dataChunks;
            _prepareSerializeJob.ChunksByteOffset = chunksByteOffset;
            _prepareSerializeJob.Samples = samples;
            _prepareSerializeJob.Run();
            
            _serializeJob.SerializedData = dataChunks;
            _serializeJob.ChunksByteOffset = chunksByteOffset;
            _serializeJob.Samples = samples;
            _serializeJob.Run(samples.Length, 128);
            
            chunksByteOffset.Dispose();
            return dataChunks;
        }
    }
}