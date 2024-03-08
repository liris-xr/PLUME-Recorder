using System.Collections.Generic;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.UI;

namespace PLUME.Base.Module.Unity.UI.CanvasScaler
{
    public class CanvasScalerFrameData : PooledFrameData<CanvasScalerFrameData>
    {
        public static readonly FrameDataPool<CanvasScalerFrameData> Pool = new();

        private readonly List<CanvasScalerCreate> _createSamples = new();
        private readonly List<CanvasScalerDestroy> _destroySamples = new();
        private readonly List<CanvasScalerUpdate> _updateSamples = new();

        public void AddCreateSamples(IEnumerable<CanvasScalerCreate> samples)
        {
            _createSamples.AddRange(samples);
        }

        public void AddDestroySamples(IEnumerable<CanvasScalerDestroy> samples)
        {
            _destroySamples.AddRange(samples);
        }

        public void AddUpdateSamples(IEnumerable<CanvasScalerUpdate> samples)
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