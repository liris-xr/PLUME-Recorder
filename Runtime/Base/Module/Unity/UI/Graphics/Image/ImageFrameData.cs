using System.Collections.Generic;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.UI;

namespace PLUME.Base.Module.Unity.UI.Graphics.Image
{
    public class ImageFrameData : PooledFrameData<ImageFrameData>
    {
        public static readonly FrameDataPool<ImageFrameData> Pool = new();

        private readonly List<ImageCreate> _createSamples = new();
        private readonly List<ImageDestroy> _destroySamples = new();
        private readonly List<ImageUpdate> _updateSamples = new();
        private readonly List<GraphicUpdate> _graphicUpdateSamples = new();
        
        public void AddCreateSamples(IEnumerable<ImageCreate> samples)
        {
            _createSamples.AddRange(samples);
        }

        public void AddDestroySamples(IEnumerable<ImageDestroy> samples)
        {
            _destroySamples.AddRange(samples);
        }

        public void AddUpdateSamples(IEnumerable<ImageUpdate> samples)
        {
            _updateSamples.AddRange(samples);
        }
        
        public void AddGraphicUpdateSamples(IEnumerable<GraphicUpdate> samples)
        {
            _graphicUpdateSamples.AddRange(samples);
        }

        public override void Serialize(FrameDataWriter frameDataWriter)
        {
            frameDataWriter.WriteManagedBatch(_createSamples);
            frameDataWriter.WriteManagedBatch(_destroySamples);
            frameDataWriter.WriteManagedBatch(_updateSamples);
            frameDataWriter.WriteManagedBatch(_graphicUpdateSamples);
        }

        public override void Clear()
        {
            _createSamples.Clear();
            _destroySamples.Clear();
            _updateSamples.Clear();
            _graphicUpdateSamples.Clear();
        }
    }
}