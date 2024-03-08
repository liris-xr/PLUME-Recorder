using System.Collections.Generic;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity;

namespace PLUME.Base.Module.Unity.Scene
{
    public class SceneFrameData : PooledFrameData<SceneFrameData>
    {
        public static readonly FrameDataPool<SceneFrameData> Pool = new();
        
        private readonly List<LoadScene> _loadSceneSamples = new();
        private readonly List<UnloadScene> _unloadScenesSamples = new();
        private ChangeActiveScene _changeActiveSceneSample;
        
        public void AddLoadSceneSamples(IEnumerable<LoadScene> samples)
        {
            _loadSceneSamples.AddRange(samples);
        }
        
        public void AddUnloadSceneSamples(IEnumerable<UnloadScene> samples)
        {
            _unloadScenesSamples.AddRange(samples);
        }
        
        public void SetChangeActiveSceneSample(ChangeActiveScene sample)
        {
            _changeActiveSceneSample = sample;
        }
        
        public override void Serialize(FrameDataWriter frameDataWriter)
        {
            frameDataWriter.WriteManagedBatch(_loadSceneSamples);
            frameDataWriter.WriteManagedBatch(_unloadScenesSamples);
            if(_changeActiveSceneSample != null)
                frameDataWriter.WriteManaged(_changeActiveSceneSample);
        }

        public override void Clear()
        {
            _loadSceneSamples.Clear();
            _unloadScenesSamples.Clear();
            _changeActiveSceneSample = null;
        }
    }
}