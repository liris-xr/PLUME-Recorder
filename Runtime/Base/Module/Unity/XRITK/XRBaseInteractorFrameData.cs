using System.Collections.Generic;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.XRITK;

namespace PLUME.Base.Module.Unity.XRITK
{
    public class XRBaseInteractorFrameData : PooledFrameData<XRBaseInteractorFrameData>
    {
        public static readonly FrameDataPool<XRBaseInteractorFrameData> Pool = new();
        
        private readonly List<XRBaseInteractorCreate> _createSamples = new();
        private readonly List<XRBaseInteractorDestroy> _destroySamples = new();
        private readonly List<XRBaseInteractorUpdate> _updateSamples = new();

        public void AddCreateSamples(IEnumerable<XRBaseInteractorCreate> samples)
        {
            _createSamples.AddRange(samples);
        }

        public void AddDestroySamples(IEnumerable<XRBaseInteractorDestroy> samples)
        {
            _destroySamples.AddRange(samples);
        }

        public void AddUpdateSamples(IEnumerable<XRBaseInteractorUpdate> samples)
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