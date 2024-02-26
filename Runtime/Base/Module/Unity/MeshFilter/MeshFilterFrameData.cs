using System;
using PLUME.Base.Module.Unity.MeshFilter.Sample;
using PLUME.Core.Recorder.Module.Frame;
using Unity.Collections;

namespace PLUME.Base.Module.Unity.MeshFilter
{
    public struct MeshFilterFrameData : IFrameData, IDisposable
    {
        private NativeList<MeshFilterUpdate> _updateSamples;
        private NativeList<MeshFilterCreate> _createSamples;
        private NativeList<MeshFilterDestroy> _destroySamples;

        public MeshFilterFrameData(NativeList<MeshFilterUpdate> updateSamples,
            NativeList<MeshFilterCreate> createSamples, NativeList<MeshFilterDestroy> destroySamples)
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
            var prepareSerializeJob = new FrameDataBatchPrepareSerializeJob<MeshFilterUpdate>();
            var serializeJob = new FrameDataBatchSerializeJob<MeshFilterUpdate>();
            var batchSerializer =
                new FrameDataBatchSerializer<MeshFilterUpdate>(prepareSerializeJob, serializeJob);
            frameDataWriter.WriteBatch(_updateSamples.AsArray(), batchSerializer);
        }

        private void SerializeCreateSamples(FrameDataWriter frameDataWriter)
        {
            var prepareSerializeJob = new FrameDataBatchPrepareSerializeJob<MeshFilterCreate>();
            var serializeJob = new FrameDataBatchSerializeJob<MeshFilterCreate>();
            var batchSerializer =
                new FrameDataBatchSerializer<MeshFilterCreate>(prepareSerializeJob, serializeJob);
            frameDataWriter.WriteBatch(_createSamples.AsArray(), batchSerializer);
        }

        private void SerializeDestroySamples(FrameDataWriter frameDataWriter)
        {
            var prepareSerializeJob = new FrameDataBatchPrepareSerializeJob<MeshFilterDestroy>();
            var serializeJob = new FrameDataBatchSerializeJob<MeshFilterDestroy>();
            var batchSerializer =
                new FrameDataBatchSerializer<MeshFilterDestroy>(prepareSerializeJob, serializeJob);
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