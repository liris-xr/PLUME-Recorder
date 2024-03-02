using System.Collections.Generic;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity;

namespace PLUME.Base.Module.Unity.GameObject
{
    public class GameObjectFrameData : PooledFrameData<GameObjectFrameData>
    {
        public static readonly FrameDataPool<GameObjectFrameData> Pool = new();

        private readonly List<GameObjectCreate> _createSamples = new();
        private readonly List<GameObjectDestroy> _destroySamples = new();
        private readonly List<GameObjectUpdate> _updateSamples = new();

        public void AddCreateSamples(IEnumerable<GameObjectCreate> samples)
        {
            _createSamples.AddRange(samples);
        }

        public void AddDestroySamples(IEnumerable<GameObjectDestroy> samples)
        {
            _destroySamples.AddRange(samples);
        }

        public void AddUpdateSamples(IEnumerable<GameObjectUpdate> samples)
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