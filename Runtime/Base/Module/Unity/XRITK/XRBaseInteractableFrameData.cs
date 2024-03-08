using System.Collections.Generic;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.XRITK;

namespace PLUME.Base.Module.Unity.XRITK
{
    public class XRBaseInteractableFrameData : PooledFrameData<XRBaseInteractableFrameData>
    {
        public static readonly FrameDataPool<XRBaseInteractableFrameData> Pool = new();
        
        private readonly List<XRBaseInteractableCreate> _createSamples = new();
        private readonly List<XRBaseInteractableDestroy> _destroySamples = new();
        private readonly List<XRBaseInteractableUpdate> _updateSamples = new();

        public void AddCreateSamples(IEnumerable<XRBaseInteractableCreate> samples)
        {
            _createSamples.AddRange(samples);
        }

        public void AddDestroySamples(IEnumerable<XRBaseInteractableDestroy> samples)
        {
            _destroySamples.AddRange(samples);
        }

        public void AddUpdateSamples(IEnumerable<XRBaseInteractableUpdate> samples)
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