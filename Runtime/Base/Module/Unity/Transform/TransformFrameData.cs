using System;
using PLUME.Core.Recorder.Module;
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

        public void Dispose()
        {
            DirtySamples.Dispose();
        }
    }
}