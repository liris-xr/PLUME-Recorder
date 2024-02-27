using System.Collections.Generic;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity;

namespace PLUME.Base.Module.Unity.MeshFilter
{
    public class MeshFilterFrameData : PooledFrameData<MeshFilterFrameData>
    {
        private readonly List<MeshFilterCreate> _createSamples = new();
        private readonly List<MeshFilterDestroy> _destroySamples = new();
        private readonly List<MeshFilterUpdate> _updateSamples = new();

        public void AddCreateSample(MeshFilterCreate sample)
        {
            _createSamples.Add(sample);
        }

        public void AddDestroySample(MeshFilterDestroy sample)
        {
            _destroySamples.Add(sample);
        }

        public void AddUpdateSample(MeshFilterUpdate sample)
        {
            _updateSamples.Add(sample);
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