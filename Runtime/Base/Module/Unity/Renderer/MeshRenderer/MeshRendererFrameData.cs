using System.Collections.Generic;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity;

namespace PLUME.Base.Module.Unity.Renderer.MeshRenderer
{
    public class MeshRendererFrameData : PooledFrameData<MeshRendererFrameData>
    {
        public static FrameDataPool<MeshRendererFrameData> Pool = new();
        
        private readonly List<MeshRendererCreate> _createSamples = new();
        private readonly List<MeshRendererDestroy> _destroySamples = new();
        private readonly List<RendererUpdate> _rendererUpdateSamples = new();
        
        public void AddCreateSamples(IEnumerable<MeshRendererCreate> samples)
        {
            _createSamples.AddRange(samples);
        }
        
        public void AddDestroySamples(IEnumerable<MeshRendererDestroy> samples)
        {
            _destroySamples.AddRange(samples);
        }
        
        public void AddUpdateSamples(IEnumerable<RendererUpdate> samples)
        {
            _rendererUpdateSamples.AddRange(samples);
        }
        
        public override void Serialize(FrameDataWriter frameDataWriter)
        {
            frameDataWriter.WriteManagedBatch(_createSamples);
            frameDataWriter.WriteManagedBatch(_destroySamples);
            frameDataWriter.WriteManagedBatch(_rendererUpdateSamples);
        }
        
        public override void Clear()
        {
            _createSamples.Clear();
            _destroySamples.Clear();
            _rendererUpdateSamples.Clear();
        }
    }
}
