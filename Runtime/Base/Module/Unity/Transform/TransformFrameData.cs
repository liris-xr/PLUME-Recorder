using System;
using PLUME.Core.Recorder.Module;
using Unity.Collections;

namespace PLUME.Base.Module.Unity.Transform
{
    public readonly struct TransformFrameData : IFrameData, IDisposable
    {
        public int DirtySamplesMaxLength { get; }
        public NativeList<TransformUpdateLocalPositionSample> DirtySamples { get; }

        public TransformFrameData(NativeList<TransformUpdateLocalPositionSample> dirtySamples, int dirtySamplesMaxLength)
        {
            DirtySamples = dirtySamples;
            DirtySamplesMaxLength = dirtySamplesMaxLength;
        }

        public void Dispose()
        {
            DirtySamples.Dispose();
        }
    }
}