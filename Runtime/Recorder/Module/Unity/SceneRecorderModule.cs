using System.Collections.Generic;
using PLUME.Sample.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityRuntimeGuid;
using LoadSceneMode = UnityEngine.SceneManagement.LoadSceneMode;

namespace PLUME
{
    // TODO: handle procedural scene creation by attaching a hook on SceneManager.CreateScene()
    public class SceneRecorderModule : RecorderModule, IStartRecordingEventReceiver, IStopRecordingEventReceiver
    {
        private Scene? _lastActiveScene;
        private readonly List<Scene> _loadedScenes = new();
        
        private readonly Dictionary<Scene, SceneIdentifier> _cachedSceneIdentifiers = new();

        public int ExecutionPriority()
        {
            return 1;
        }
        
        protected override void ResetCache()
        {
            _loadedScenes.Clear();
            _lastActiveScene = null;
            _cachedSceneIdentifiers.Clear();
        }

        public new void OnStartRecording()
        {
            base.OnStartRecording();
            
            SceneManager.sceneLoaded += OnLoadScene;
            SceneManager.sceneUnloaded += OnUnloadScene;
            SceneManager.activeSceneChanged += OnChangeActiveScene;

            for (var sceneIdx = 0; sceneIdx < SceneManager.sceneCount; ++sceneIdx)
            {
                var scene = SceneManager.GetSceneAt(sceneIdx);
                if(scene.isLoaded)
                    RecordLoadScene(scene, SceneManager.sceneCount > 1 ? LoadSceneMode.Additive : LoadSceneMode.Single);
            }

            RecordChangeActiveScene(SceneManager.GetActiveScene());
        }

        public void OnStopRecording()
        {
            SceneManager.sceneLoaded -= OnLoadScene;
            SceneManager.sceneUnloaded -= OnUnloadScene;
            SceneManager.activeSceneChanged -= OnChangeActiveScene;
        }

        private void OnLoadScene(Scene scene, LoadSceneMode mode)
        {
            RecordLoadScene(scene, mode);
            
            if (!_loadedScenes.Contains(scene))
                _loadedScenes.Add(scene);

            if (SceneManager.GetActiveScene() != scene || _lastActiveScene == scene) return;
            
            RecordChangeActiveScene(scene);
            _lastActiveScene = scene;
        }

        private void OnUnloadScene(Scene scene)
        {
            RecordUnloadScene(scene);
            _loadedScenes.Remove(scene);

            if (SceneManager.GetActiveScene() == _lastActiveScene) return;
            
            RecordChangeActiveScene(SceneManager.GetActiveScene());
            _lastActiveScene = scene;
        }

        private void OnChangeActiveScene(Scene oldActive, Scene newActive)
        {
            // As OnChangeActiveScene is fired before OnLoadScene, we make sure that the scene is already loaded to record
            // this change. Otherwise it means that this is called by a scene being loaded in single mode, thus the event
            // is recorded by the OnLoadScene event.
            if (!_loadedScenes.Contains(newActive))
                return;

            if (newActive == _lastActiveScene) return;
            RecordChangeActiveScene(newActive);
            _lastActiveScene = newActive;
        }
        
        private static int GetSceneRuntimeIndex(Scene scene)
        {
            for (var sceneIdx = 0; sceneIdx < SceneManager.sceneCount; ++sceneIdx)
            {
                if (SceneManager.GetSceneAt(sceneIdx) == scene)
                    return sceneIdx;
            }

            return -1;
        }
        
        private void RecordLoadScene(Scene scene, LoadSceneMode mode)
        {
            var guidRegistry = SceneGuidRegistry.GetOrCreate(scene);

            var sceneRuntimeIndex = GetSceneRuntimeIndex(scene);
            
            var sceneIdentifier = new SceneIdentifier
            {
                Id = guidRegistry.SceneGuid,
                BuildIndex = scene.buildIndex,
                RuntimeIndex = sceneRuntimeIndex.ToString(),
                Path = scene.path,
                Mode = mode.ToPayload(),
                Name = scene.name
            };
            
            _cachedSceneIdentifiers[scene] = sceneIdentifier;
            
            var loadSceneSample = new LoadScene
            {
                Id = sceneIdentifier
            };

            Recorder.Instance.RecordSampleStamped(loadSceneSample);
        }

        private void RecordUnloadScene(Scene scene)
        {
            if (!_cachedSceneIdentifiers.TryGetValue(scene, out var sceneIdentifier)) return;
            
            var unloadSceneSample = new UnloadScene
            {
                Id = sceneIdentifier,
            };

            Recorder.Instance.RecordSampleStamped(unloadSceneSample);
        }

        private void RecordChangeActiveScene(Scene scene)
        {
            if (!_cachedSceneIdentifiers.TryGetValue(scene, out var sceneIdentifier)) return;
            
            var changeActiveScene = new ChangeActiveScene
            {
                Id = sceneIdentifier
            };

            Recorder.Instance.RecordSampleStamped(changeActiveScene);
        }
    }
}