using System.Collections.Generic;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity;

namespace PLUME.Base.Module.Unity.Renderer.LineRenderer
{
    public class LineRendererFrameData : PooledFrameData<LineRendererFrameData>
    {
        public static readonly FrameDataPool<LineRendererFrameData> Pool = new();

        private readonly List<LineRendererCreate> _createSamples = new();
        private readonly List<LineRendererDestroy> _destroySamples = new();
        private readonly List<LineRendererUpdate> _updateSamples = new();
        private readonly List<RendererUpdate> _rendererUpdateSamples = new();

        public void AddCreateSamples(IEnumerable<LineRendererCreate> samples)
        {
            _createSamples.AddRange(samples);
        }

        public void AddDestroySamples(IEnumerable<LineRendererDestroy> samples)
        {
            _destroySamples.AddRange(samples);
        }

        public void AddUpdateSamples(IEnumerable<LineRendererUpdate> samples)
        {
            _updateSamples.AddRange(samples);
        }
        
        public void AddUpdateSamples(IEnumerable<RendererUpdate> samples)
        {
            _rendererUpdateSamples.AddRange(samples);
        }

        public override void Serialize(FrameDataWriter frameDataWriter)
        {
            frameDataWriter.WriteManagedBatch(_createSamples);
            frameDataWriter.WriteManagedBatch(_destroySamples);
            frameDataWriter.WriteManagedBatch(_updateSamples);
            frameDataWriter.WriteManagedBatch(_rendererUpdateSamples);
        }

        public override void Clear()
        {
            _createSamples.Clear();
            _destroySamples.Clear();
            _updateSamples.Clear();
            _rendererUpdateSamples.Clear();
        }
    }
}