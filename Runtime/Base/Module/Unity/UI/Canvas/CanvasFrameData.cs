using System.Collections.Generic;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.UI;

namespace PLUME.Base.Module.Unity.UI.Canvas
{
    public class CanvasFrameData : PooledFrameData<CanvasFrameData>
    {
        public static readonly FrameDataPool<CanvasFrameData> Pool = new();

        private readonly List<CanvasCreate> _createSamples = new();
        private readonly List<CanvasDestroy> _destroySamples = new();
        private readonly List<CanvasUpdate> _updateSamples = new();

        public void AddCreateSamples(IEnumerable<CanvasCreate> samples)
        {
            _createSamples.AddRange(samples);
        }

        public void AddDestroySamples(IEnumerable<CanvasDestroy> samples)
        {
            _destroySamples.AddRange(samples);
        }

        public void AddUpdateSamples(IEnumerable<CanvasUpdate> samples)
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