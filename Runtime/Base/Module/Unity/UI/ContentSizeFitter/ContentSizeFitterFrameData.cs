using System.Collections.Generic;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.UI;

namespace PLUME.Base.Module.Unity.UI.ContentSizeFitter
{
    public class ContentSizeFitterFrameData : PooledFrameData<ContentSizeFitterFrameData>
    {
        public static readonly FrameDataPool<ContentSizeFitterFrameData> Pool = new();

        private readonly List<ContentSizeFitterCreate> _createSamples = new();
        private readonly List<ContentSizeFitterDestroy> _destroySamples = new();
        private readonly List<ContentSizeFitterUpdate> _updateSamples = new();

        public void AddCreateSamples(IEnumerable<ContentSizeFitterCreate> samples)
        {
            _createSamples.AddRange(samples);
        }

        public void AddDestroySamples(IEnumerable<ContentSizeFitterDestroy> samples)
        {
            _destroySamples.AddRange(samples);
        }

        public void AddUpdateSamples(IEnumerable<ContentSizeFitterUpdate> samples)
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