using System.Collections.Generic;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity;

namespace PLUME.Base.Module.Unity.Terrain
{
    public class TerrainFrameData : PooledFrameData<TerrainFrameData>
    {
        public static FrameDataPool<TerrainFrameData> Pool = new();

        private readonly List<TerrainCreate> _createSamples = new();
        private readonly List<TerrainDestroy> _destroySamples = new();
        private readonly List<TerrainUpdate> _updateSamples = new();

        public void AddCreateSamples(IEnumerable<TerrainCreate> samples)
        {
            _createSamples.AddRange(samples);
        }

        public void AddDestroySamples(IEnumerable<TerrainDestroy> samples)
        {
            _destroySamples.AddRange(samples);
        }

        public void AddUpdateSamples(IEnumerable<TerrainUpdate> samples)
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