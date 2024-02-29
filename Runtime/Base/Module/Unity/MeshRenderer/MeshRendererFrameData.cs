using System.Collections.Generic;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity;

namespace PLUME
{
    public class MeshRendererFrameData : PooledFrameData<MeshRendererFrameData>
    {
        private readonly List<MeshRendererCreate> _createSamples = new();
        private readonly List<MeshRendererDestroy> _destroySamples = new();
        private readonly List<MeshRendererUpdate> _updateSamples = new();
        
        public void AddCreateSample(MeshRendererCreate sample)
        {
            _createSamples.Add(sample);
        }
        
        public void AddDestroySample(MeshRendererDestroy sample)
        {
            _destroySamples.Add(sample);
        }
        
        public void AddUpdateSample(MeshRendererUpdate sample)
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
