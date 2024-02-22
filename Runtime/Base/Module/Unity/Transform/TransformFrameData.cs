using System;
using PLUME.Core.Recorder.Module.Frame;
using Unity.Collections;

namespace PLUME.Base.Module.Unity.Transform
{
    public readonly struct TransformFrameData : IFrameData, IDisposable
    {
        public NativeList<TransformUpdateLocalPositionSample> DirtySamples { get; }

        public TransformFrameData(NativeList<TransformUpdateLocalPositionSample> dirtySamples)
        {
            DirtySamples = dirtySamples;
        }

        public void Serialize(FrameDataWriter frameDataWriter)
        {
            var prepareSerializeJob = new FrameDataBatchPrepareSerializeJob<TransformUpdateLocalPositionSample>();
            var serializeJob = new FrameDataBatchSerializeJob<TransformUpdateLocalPositionSample>();
            var batchSerializer =
                new FrameDataBatchSerializer<TransformUpdateLocalPositionSample>(prepareSerializeJob, serializeJob);
            frameDataWriter.WriteBatch(DirtySamples.AsArray(), batchSerializer);
        }

        public void Dispose()
        {
            DirtySamples.Dispose();
        }
    }
}