#if HDRP_ENABLED
using System.Collections.Generic;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.HDRP;

namespace PLUME.Base.Module.Unity.HDRP
{
    public class HDAdditionalLightDataFrameData : PooledFrameData<HDAdditionalLightDataFrameData>
    {
        public static readonly FrameDataPool<HDAdditionalLightDataFrameData> Pool = new();

        private readonly List<HDAdditionalLightDataCreate> _createSamples = new();
        private readonly List<HDAdditionalLightDataDestroy> _destroySamples = new();
        private readonly List<HDAdditionalLightDataUpdate> _updateSamples = new();

        public void AddCreateSamples(IEnumerable<HDAdditionalLightDataCreate> samples)
        {
            _createSamples.AddRange(samples);
        }

        public void AddDestroySamples(IEnumerable<HDAdditionalLightDataDestroy> samples)
        {
            _destroySamples.AddRange(samples);
        }

        public void AddUpdateSamples(IEnumerable<HDAdditionalLightDataUpdate> samples)
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
#endif
