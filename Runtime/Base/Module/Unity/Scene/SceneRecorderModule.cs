using System.Collections.Generic;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Core.Utils;
using PLUME.Sample.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;
using UnityRuntimeGuid;
using LoadSceneMode = UnityEngine.SceneManagement.LoadSceneMode;

namespace PLUME.Base.Module.Unity.Scene
{
    // TODO: handle procedural scene creation by creating a hook on SceneManager.CreateScene()
    [Preserve]
    public class SceneRecorderModule : FrameDataRecorderModule<SceneFrameData>
    {
        private UnityEngine.SceneManagement.Scene? _lastActiveScene;
        private readonly List<UnityEngine.SceneManagement.Scene> _loadedScenes = new();
        private readonly Dictionary<UnityEngine.SceneManagement.Scene, SceneIdentifier> _cachedSceneIdentifiers = new();

        private readonly List<LoadScene> _loadSceneSamples = new();
        private readonly List<UnloadScene> _unloadSceneSamples = new();
        private ChangeActiveScene _changeActiveSceneSample;

        protected override void OnStartRecording(RecorderContext ctx)
        {
            base.OnStartRecording(ctx);

            SceneManager.sceneLoaded += OnLoadScene;
            SceneManager.sceneUnloaded += OnUnloadScene;
            SceneManager.activeSceneChanged += OnChangeActiveScene;

            // TODO: create a function StartRecordingScene which takes a bool param to record the loading sample or not for pre-serialization
            for (var sceneIdx = 0; sceneIdx < SceneManager.sceneCount; ++sceneIdx)
            {
                var scene = SceneManager.GetSceneAt(sceneIdx);
                if (scene.isLoaded)
                    RecordLoadScene(scene, SceneManager.sceneCount > 1 ? LoadSceneMode.Additive : LoadSceneMode.Single);
            }

            RecordChangeActiveScene(SceneManager.GetActiveScene());
        }

        protected override void OnStopRecording(RecorderContext ctx)
        {
            base.OnStopRecording(ctx);

            SceneManager.sceneLoaded -= OnLoadScene;
            SceneManager.sceneUnloaded -= OnUnloadScene;
            SceneManager.activeSceneChanged -= OnChangeActiveScene;

            _loadedScenes.Clear();
            _lastActiveScene = null;
            _cachedSceneIdentifiers.Clear();
        }

        protected override SceneFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            var sceneFrameData = SceneFrameData.Pool.Get();
            sceneFrameData.AddLoadSceneSamples(_loadSceneSamples);
            sceneFrameData.AddUnloadSceneSamples(_unloadSceneSamples);
            sceneFrameData.SetChangeActiveSceneSample(_changeActiveSceneSample);
            return sceneFrameData;
        }

        protected override void AfterCollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            base.AfterCollectFrameData(frameInfo, ctx);
            _loadSceneSamples.Clear();
            _unloadSceneSamples.Clear();
            _changeActiveSceneSample = null;
        }

        private void OnLoadScene(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
        {
            RecordLoadScene(scene, mode);

            if (!_loadedScenes.Contains(scene))
                _loadedScenes.Add(scene);

            if (SceneManager.GetActiveScene() != scene || _lastActiveScene == scene)
                return;

            RecordChangeActiveScene(scene);
            _lastActiveScene = scene;
        }

        private void OnUnloadScene(UnityEngine.SceneManagement.Scene scene)
        {
            RecordUnloadScene(scene);
            _loadedScenes.Remove(scene);

            if (SceneManager.GetActiveScene() == _lastActiveScene)
                return;

            RecordChangeActiveScene(SceneManager.GetActiveScene());
            _lastActiveScene = scene;
        }

        private void OnChangeActiveScene(UnityEngine.SceneManagement.Scene oldActive,
            UnityEngine.SceneManagement.Scene newActive)
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

        private void RecordLoadScene(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
        {
            var sceneIdentifier = SampleUtils.GetSceneIdentifierPayload(scene);

            _cachedSceneIdentifiers[scene] = sceneIdentifier;

            var loadSceneSample = new LoadScene
            {
                Scene = sceneIdentifier,
                Mode = mode.ToPayload()
            };

            _loadSceneSamples.Add(loadSceneSample);
        }

        private void RecordUnloadScene(UnityEngine.SceneManagement.Scene scene)
        {
            if (!_cachedSceneIdentifiers.TryGetValue(scene, out var sceneIdentifier)) return;

            var unloadSceneSample = new UnloadScene
            {
                Scene = sceneIdentifier
            };

            _unloadSceneSamples.Add(unloadSceneSample);
        }

        private void RecordChangeActiveScene(UnityEngine.SceneManagement.Scene scene)
        {
            if (!_cachedSceneIdentifiers.TryGetValue(scene, out var sceneIdentifier)) return;

            var changeActiveScene = new ChangeActiveScene
            {
                Scene = sceneIdentifier
            };

            _changeActiveSceneSample = changeActiveScene;
        }
    }
}