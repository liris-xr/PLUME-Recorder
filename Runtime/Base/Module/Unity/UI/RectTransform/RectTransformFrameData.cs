using System.Collections.Generic;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity;
using PLUME.Sample.Unity.UI;

namespace PLUME.Base.Module.Unity.UI.RectTransform
{
    public class RectTransformFrameData : PooledFrameData<RectTransformFrameData>
    {
        public static readonly FrameDataPool<RectTransformFrameData> Pool = new();

        private readonly List<RectTransformCreate> _createSamples = new();
        private readonly List<RectTransformDestroy> _destroySamples = new();
        private readonly List<RectTransformUpdate> _updateSamples = new();
        private readonly List<TransformUpdate> _transformUpdateSamples = new();

        public void AddCreateSamples(IEnumerable<RectTransformCreate> samples)
        {
            _createSamples.AddRange(samples);
        }

        public void AddDestroySamples(IEnumerable<RectTransformDestroy> samples)
        {
            _destroySamples.AddRange(samples);
        }

        public void AddUpdateSamples(IEnumerable<RectTransformUpdate> samples)
        {
            _updateSamples.AddRange(samples);
        }
        
        public void AddTransformUpdateSamples(IEnumerable<TransformUpdate> samples)
        {
            _transformUpdateSamples.AddRange(samples);
        }

        public override void Serialize(FrameDataWriter frameDataWriter)
        {
            frameDataWriter.WriteManagedBatch(_createSamples);
            frameDataWriter.WriteManagedBatch(_destroySamples);
            frameDataWriter.WriteManagedBatch(_updateSamples);
            frameDataWriter.WriteManagedBatch(_transformUpdateSamples);
        }

        public override void Clear()
        {
            _createSamples.Clear();
            _destroySamples.Clear();
            _updateSamples.Clear();
            _transformUpdateSamples.Clear();
        }
    }
}