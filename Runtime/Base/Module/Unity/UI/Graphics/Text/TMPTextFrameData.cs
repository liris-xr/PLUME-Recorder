using System.Collections.Generic;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.UI;

namespace PLUME.Base.Module.Unity.UI.Graphics.Text
{
    public class TMPTextFrameData : PooledFrameData<TMPTextFrameData>
    {
        public static readonly FrameDataPool<TMPTextFrameData> Pool = new();

        private readonly List<TMPTextCreate> _createSamples = new();
        private readonly List<TMPTextDestroy> _destroySamples = new();
        private readonly List<TMPTextUpdate> _updateSamples = new();
        private readonly List<GraphicUpdate> _graphicUpdateSamples = new();

        public void AddCreateSamples(IEnumerable<TMPTextCreate> samples)
        {
            _createSamples.AddRange(samples);
        }

        public void AddDestroySamples(IEnumerable<TMPTextDestroy> samples)
        {
            _destroySamples.AddRange(samples);
        }

        public void AddUpdateSamples(IEnumerable<TMPTextUpdate> samples)
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