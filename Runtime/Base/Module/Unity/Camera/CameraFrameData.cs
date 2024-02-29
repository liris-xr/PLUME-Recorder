using System.Collections.Generic;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity;

namespace PLUME
{
    public class CameraFrameData : PooledFrameData<MeshRendererFrameData>
    {
        //Copy MeshFilterFrameData to CameraFrameData
        private readonly List<CameraCreate> _createSamples = new();
        private readonly List<CameraDestroy> _destroySamples = new();
        private readonly List<CameraUpdate> _updateSamples = new();
        
        public void AddCreateSample(CameraCreate sample)
        {
            _createSamples.Add(sample);
        }
        
        public void AddDestroySample(CameraDestroy sample)
        {
            _destroySamples.Add(sample);
        }
        
        public void AddUpdateSample(CameraUpdate sample)
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