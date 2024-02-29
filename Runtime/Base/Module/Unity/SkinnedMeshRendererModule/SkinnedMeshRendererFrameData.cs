using System.Collections.Generic;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity;

namespace PLUME.Base.Module.Unity.SkinnedMeshRendererModule
{
    public class SkinnedMeshRendererFrameData : PooledFrameData<SkinnedMeshRendererFrameData>
    {
        //Copy MeshFilterFrameData to MeshRendererFrameData
        private readonly List<SkinnedMeshRendererCreate> _createSamples = new();
        private readonly List<SkinnedMeshRendererDestroy> _destroySamples = new();
        private readonly List<SkinnedMeshRendererUpdate> _updateSamples = new();
        
        public void AddCreateSample(SkinnedMeshRendererCreate sample)
        {
            _createSamples.Add(sample);
        }
        
        public void AddDestroySample(SkinnedMeshRendererDestroy sample)
        {
            _destroySamples.Add(sample);
        }
        
        public void AddUpdateSample(SkinnedMeshRendererUpdate sample)
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
