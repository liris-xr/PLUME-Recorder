#if CORE_RP_ENABLED
using System.Collections.Generic;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.URP;

namespace PLUME.Base.Module.Unity.Volume
{
    public class VolumeFrameData : PooledFrameData<VolumeFrameData>
    {
        public static readonly FrameDataPool<VolumeFrameData> Pool = new();

        private readonly List<VolumeCreate> _createSamples = new();
        private readonly List<VolumeDestroy> _destroySamples = new();
        private readonly List<VolumeUpdate> _updateSamples = new();
        private readonly List<VolumeUpdateEnabled> _updateEnabledSamples = new();

        public void AddCreateSamples(IEnumerable<VolumeCreate> samples)
        {
            _createSamples.AddRange(samples);
        }

        public void AddDestroySamples(IEnumerable<VolumeDestroy> samples)
        {
            _destroySamples.AddRange(samples);
        }

        public void AddUpdateSamples(IEnumerable<VolumeUpdate> samples)
        {
            _updateSamples.AddRange(samples);
        }

        public void AddUpdateEnabledSamples(IEnumerable<VolumeUpdateEnabled> samples)
        {
            _updateEnabledSamples.AddRange(samples);
        }

        public override void Serialize(FrameDataWriter frameDataWriter)
        {
            frameDataWriter.WriteManagedBatch(_createSamples);
            frameDataWriter.WriteManagedBatch(_destroySamples);
            frameDataWriter.WriteManagedBatch(_updateSamples);
            frameDataWriter.WriteManagedBatch(_updateEnabledSamples);
        }

        public override void Clear()
        {
            _createSamples.Clear();
            _destroySamples.Clear();
            _updateSamples.Clear();
            _updateEnabledSamples.Clear();
        }
    }
}
#endif
