using System.Collections.Generic;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.UI;

namespace PLUME.Base.Module.Unity.UI.Graphics.Text
{
    public class TextFrameData : PooledFrameData<TextFrameData>
    {
        public static readonly FrameDataPool<TextFrameData> Pool = new();

        private readonly List<TextCreate> _createSamples = new();
        private readonly List<TextDestroy> _destroySamples = new();
        private readonly List<TextUpdate> _updateSamples = new();
        private readonly List<GraphicUpdate> _graphicUpdateSamples = new();

        public void AddCreateSamples(IEnumerable<TextCreate> samples)
        {
            _createSamples.AddRange(samples);
        }

        public void AddDestroySamples(IEnumerable<TextDestroy> samples)
        {
            _destroySamples.AddRange(samples);
        }

        public void AddUpdateSamples(IEnumerable<TextUpdate> samples)
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