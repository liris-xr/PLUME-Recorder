#if HDRP_ENABLED
using System.Collections.Generic;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.HDRP;

namespace PLUME.Base.Module.Unity.HDRP
{
    public class HDAdditionalCameraDataFrameData : PooledFrameData<HDAdditionalCameraDataFrameData>
    {
        public static readonly FrameDataPool<HDAdditionalCameraDataFrameData> Pool = new();

        private readonly List<HDAdditionalCameraDataCreate> _createSamples = new();
        private readonly List<HDAdditionalCameraDataDestroy> _destroySamples = new();
        private readonly List<HDAdditionalCameraDataUpdate> _updateSamples = new();

        public void AddCreateSamples(IEnumerable<HDAdditionalCameraDataCreate> samples)
        {
            _createSamples.AddRange(samples);
        }

        public void AddDestroySamples(IEnumerable<HDAdditionalCameraDataDestroy> samples)
        {
            _destroySamples.AddRange(samples);
        }

        public void AddUpdateSamples(IEnumerable<HDAdditionalCameraDataUpdate> samples)
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
