using System.Collections.Generic;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.UI;

namespace PLUME.Base.Module.Unity.UI.CanvasRenderer
{
    public class CanvasRendererFrameData : PooledFrameData<CanvasRendererFrameData>
    {
        public static readonly FrameDataPool<CanvasRendererFrameData> Pool = new();

        private readonly List<CanvasRendererCreate> _createSamples = new();
        private readonly List<CanvasRendererDestroy> _destroySamples = new();
        private readonly List<CanvasRendererUpdate> _updateSamples = new();

        public void AddCreateSamples(IEnumerable<CanvasRendererCreate> samples)
        {
            _createSamples.AddRange(samples);
        }

        public void AddDestroySamples(IEnumerable<CanvasRendererDestroy> samples)
        {
            _destroySamples.AddRange(samples);
        }

        public void AddUpdateSamples(IEnumerable<CanvasRendererUpdate> samples)
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