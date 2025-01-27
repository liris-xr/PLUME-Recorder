using System.Collections.Generic;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity;

namespace PLUME.Base.Module.Unity.ReflectionProbe
{
    public class ReflectionProbeFrameData : PooledFrameData<ReflectionProbeFrameData>
    {
        public static readonly FrameDataPool<ReflectionProbeFrameData> Pool = new();
        
        private readonly List<ReflectionProbeCreate> _createSamples = new();
        private readonly List<ReflectionProbeDestroy> _destroySamples = new();
        private readonly List<ReflectionProbeUpdate> _updateSamples = new();

        public void AddCreateSamples(IEnumerable<ReflectionProbeCreate> samples)
        {
            _createSamples.AddRange(samples);
        }

        public void AddDestroySamples(IEnumerable<ReflectionProbeDestroy> samples)
        {
            _destroySamples.AddRange(samples);
        }

        public void AddUpdateSamples(IEnumerable<ReflectionProbeUpdate> samples)
        {
            _updateSamples.AddRange(samples);
        }

        public override void Serialize(FrameDataWriter frameDataWriter)
        {
            frameDataWriter.WriteManagedBatch(_createSamples);
            frameDataWriter.WriteManagedBatch(_destroySamples);
            frameDataWriter.WriteManagedBatch(_updateSamples);
        }

        public override void Clear()
        {
            _createSamples.Clear();
            _destroySamples.Clear();
            _updateSamples.Clear();
        }
    }
}