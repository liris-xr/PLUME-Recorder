using System.Collections.Generic;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity;

namespace PLUME.Base.Module.Unity.Renderer.SkinnedMeshRendererModule
{
    public class SkinnedMeshRendererFrameData : PooledFrameData<SkinnedMeshRendererFrameData>
    {
        public static readonly FrameDataPool<SkinnedMeshRendererFrameData> Pool = new();
        
        //Copy MeshFilterFrameData to MeshRendererFrameData
        private readonly List<SkinnedMeshRendererCreate> _createSamples = new();
        private readonly List<SkinnedMeshRendererDestroy> _destroySamples = new();
        private readonly List<SkinnedMeshRendererUpdate> _updateSamples = new();
        private readonly List<RendererUpdate> _rendererUpdateSamples = new();
        
        public void AddCreateSamples(IEnumerable<SkinnedMeshRendererCreate> samples)
        {
            _createSamples.AddRange(samples);
        }
        
        public void AddDestroySamples(IEnumerable<SkinnedMeshRendererDestroy> samples)
        {
            _destroySamples.AddRange(samples);
        }
        
        public void AddUpdateSamples(IEnumerable<SkinnedMeshRendererUpdate> samples)
        {
            _updateSamples.AddRange(samples);
        }
        
        public void AddUpdateSamples(IEnumerable<RendererUpdate> samples)
        {
            _rendererUpdateSamples.AddRange(samples);
        }
        
        public override void Serialize(FrameDataWriter frameDataWriter)
        {
            frameDataWriter.WriteManagedBatch(_createSamples);
            frameDataWriter.WriteManagedBatch(_destroySamples);
            frameDataWriter.WriteManagedBatch(_updateSamples);
            frameDataWriter.WriteManagedBatch(_rendererUpdateSamples);
        }
        
        public override void Clear()
        {
            _createSamples.Clear();
            _destroySamples.Clear();
            _updateSamples.Clear();
            _rendererUpdateSamples.Clear();
        }
    }
}
