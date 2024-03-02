using System.Collections.Generic;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity;

namespace PLUME.Base.Module.Unity.Light
{
    public class LightFrameData : PooledFrameData<LightFrameData>
    {
        public static readonly FrameDataPool<LightFrameData> Pool = new();

        private readonly List<LightCreate> _createSamples = new();
        private readonly List<LightDestroy> _destroySamples = new();
        private readonly List<LightUpdate> _updateSamples = new();

        public void AddCreateSamples(IEnumerable<LightCreate> samples)
        {
            _createSamples.AddRange(samples);
        }

        public void AddDestroySamples(IEnumerable<LightDestroy> samples)
        {
            _destroySamples.AddRange(samples);
        }

        public void AddUpdateSamples(IEnumerable<LightUpdate> samples)
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