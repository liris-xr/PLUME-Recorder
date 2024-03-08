using System;
using PLUME.Base.Module.Unity.Transform.Sample;
using PLUME.Core.Recorder.Module.Frame;
using Unity.Collections;

namespace PLUME.Base.Module.Unity.Transform
{
    public struct TransformFrameData : IFrameData, IDisposable
    {
        private NativeList<TransformUpdate> _updateSamples;
        private NativeList<TransformCreate> _createSamples;
        private NativeList<TransformDestroy> _destroySamples;

        public TransformFrameData(NativeList<TransformUpdate> updateSamples,
            NativeList<TransformCreate> createSamples,
            NativeList<TransformDestroy> destroySamples)
        {
            _updateSamples = updateSamples;
            _createSamples = createSamples;
            _destroySamples = destroySamples;
        }

        public void Serialize(FrameDataWriter frameDataWriter)
        {
            SerializeCreateSamples(frameDataWriter);
            SerializeDestroySamples(frameDataWriter);
            SerializeUpdateSamples(frameDataWriter);
        }

        private void SerializeUpdateSamples(FrameDataWriter frameDataWriter)
        {
            var prepareSerializeJob = new FrameDataBatchPrepareSerializeJob<TransformUpdate>();
            var serializeJob = new FrameDataBatchSerializeJob<TransformUpdate>();
            var batchSerializer =
                new FrameDataBatchSerializer<TransformUpdate>(prepareSerializeJob, serializeJob);
            frameDataWriter.WriteBatch(_updateSamples.AsArray(), batchSerializer);
        }
        
        private void SerializeCreateSamples(FrameDataWriter frameDataWriter)
        {
            var prepareSerializeJob = new FrameDataBatchPrepareSerializeJob<TransformCreate>();
            var serializeJob = new FrameDataBatchSerializeJob<TransformCreate>();
            var batchSerializer =
                new FrameDataBatchSerializer<TransformCreate>(prepareSerializeJob, serializeJob);
            frameDataWriter.WriteBatch(_createSamples.AsArray(), batchSerializer);
        }
        
        private void SerializeDestroySamples(FrameDataWriter frameDataWriter)
        {
            var prepareSerializeJob = new FrameDataBatchPrepareSerializeJob<TransformDestroy>();
            var serializeJob = new FrameDataBatchSerializeJob<TransformDestroy>();
            var batchSerializer =
                new FrameDataBatchSerializer<TransformDestroy>(prepareSerializeJob, serializeJob);
            frameDataWriter.WriteBatch(_destroySamples.AsArray(), batchSerializer);
        }

        public void Dispose()
        {
            _updateSamples.Dispose();
            _createSamples.Dispose();
            _destroySamples.Dispose();
        }
    }
}