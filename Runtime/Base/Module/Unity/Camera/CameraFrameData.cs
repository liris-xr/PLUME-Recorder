using System.Collections.Generic;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity;

namespace PLUME.Base.Module.Unity.Camera
{
    public class CameraFrameData : PooledFrameData<CameraFrameData>
    {
        public static readonly FrameDataPool<CameraFrameData> Pool = new();
        
        //Copy MeshFilterFrameData to CameraFrameData
        private readonly List<CameraCreate> _createSamples = new();
        private readonly List<CameraDestroy> _destroySamples = new();
        private readonly List<CameraUpdate> _updateSamples = new();

        public void AddCreateSamples(IEnumerable<CameraCreate> samples)
        {
            _createSamples.AddRange(samples);
        }

        public void AddDestroySamples(IEnumerable<CameraDestroy> samples)
        {
            _destroySamples.AddRange(samples);
        }

        public void AddUpdateSamples(IEnumerable<CameraUpdate> samples)
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