using System.Collections.Generic;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity;

namespace PLUME.Base.Module.Unity.MeshFilter
{
    public class MeshFilterFrameData : IFrameData
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

        public void Serialize(FrameDataWriter frameDataWriter)
        {
            frameDataWriter.WriteManagedBatch(_createSamples);
            frameDataWriter.WriteManagedBatch(_destroySamples);
            frameDataWriter.WriteManagedBatch(_updateSamples);
        }
    }
}