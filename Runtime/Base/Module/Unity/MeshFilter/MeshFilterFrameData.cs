using System.Collections.Generic;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity;

namespace PLUME.Base.Module.Unity.MeshFilter
{
    public class MeshFilterFrameData : PooledFrameData<MeshFilterFrameData>
    {
        public static readonly FrameDataPool<MeshFilterFrameData> Pool = new();
        
        private readonly List<MeshFilterCreate> _createSamples = new();
        private readonly List<MeshFilterDestroy> _destroySamples = new();
        private readonly List<MeshFilterUpdate> _updateSamples = new();

        public void AddCreateSamples(IEnumerable<MeshFilterCreate> samples)
        {
            _createSamples.AddRange(samples);
        }

        public void AddDestroySamples(IEnumerable<MeshFilterDestroy> samples)
        {
            _destroySamples.AddRange(samples);
        }

        public void AddUpdateSamples(IEnumerable<MeshFilterUpdate> samples)
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