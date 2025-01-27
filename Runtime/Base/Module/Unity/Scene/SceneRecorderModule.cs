using System.Collections.Generic;
using System.Linq;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Core.Utils;
using PLUME.Sample.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;
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

        private UnityEngine.GameObject _dontDestroyOnLoadHandle;
        
        protected override void OnStartRecording(RecorderContext ctx)
        {
            base.OnStartRecording(ctx);

            SceneManager.sceneLoaded += (scene, loadMode) => OnLoadScene(scene, loadMode, ctx);
            SceneManager.sceneUnloaded += scene => OnUnloadScene(scene, ctx);
            SceneManager.activeSceneChanged += (scene, loadMode) => OnChangeActiveScene(scene, loadMode, ctx);
            
            _dontDestroyOnLoadHandle = new UnityEngine.GameObject("DontDestroyOnLoadHandle");
            Object.DontDestroyOnLoad(_dontDestroyOnLoadHandle);
            OnLoadScene(_dontDestroyOnLoadHandle.scene, LoadSceneMode.Additive, ctx);
            
            // TODO: create a function StartRecordingScene which takes a bool param to record the loading sample or not for pre-serialization
            for (var sceneIdx = 0; sceneIdx < SceneManager.sceneCount; ++sceneIdx)
            {
                var scene = SceneManager.GetSceneAt(sceneIdx);
                var loadMode = SceneManager.sceneCount > 1 ? LoadSceneMode.Additive : LoadSceneMode.Single;
                if (scene.isLoaded)
                {
                    RecordLoadScene(scene, loadMode, ctx);
                    OnLoadScene(scene, SceneManager.sceneCount > 1 ? LoadSceneMode.Additive : LoadSceneMode.Single, ctx);
                }
            }

            RecordChangeActiveScene(SceneManager.GetActiveScene(), ctx);
        }

        protected override void OnStopRecording(RecorderContext ctx)
        {
            base.OnStopRecording(ctx);

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

        private void OnLoadScene(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode, RecorderContext ctx)
        {
            RecordLoadScene(scene, mode, ctx);

            if (!_loadedScenes.Contains(scene))
                _loadedScenes.Add(scene);

            if (SceneManager.GetActiveScene() != scene || _lastActiveScene == scene)
                return;

            RecordChangeActiveScene(scene, ctx);
            _lastActiveScene = scene;
        }

        private void OnUnloadScene(UnityEngine.SceneManagement.Scene scene, RecorderContext ctx)
        {
            RecordUnloadScene(scene, ctx);
            _loadedScenes.Remove(scene);

            if (SceneManager.GetActiveScene() == _lastActiveScene)
                return;

            RecordChangeActiveScene(SceneManager.GetActiveScene(), ctx);
            _lastActiveScene = scene;
        }

        private void OnChangeActiveScene(UnityEngine.SceneManagement.Scene oldActive,
            UnityEngine.SceneManagement.Scene newActive, RecorderContext ctx)
        {
            // As OnChangeActiveScene is fired before OnLoadScene, we make sure that the scene is already loaded to record
            // this change. Otherwise it means that this is called by a scene being loaded in single mode, thus the event
            // is recorded by the OnLoadScene event.
            if (!_loadedScenes.Contains(newActive))
                return;

            if (newActive == _lastActiveScene) return;
            RecordChangeActiveScene(newActive, ctx);
            _lastActiveScene = newActive;
        }

        private void RecordLoadScene(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode, RecorderContext ctx)
        {
            var sceneIdentifier = SampleUtils.GetSceneIdentifierPayload(scene);

            _cachedSceneIdentifiers[scene] = sceneIdentifier;

            var loadSceneSample = new LoadScene
            {
                Scene = sceneIdentifier,
                Mode = mode.ToPayload()
            };

            _loadSceneSamples.Add(loadSceneSample);

            var gameObjects = Object.FindObjectsByType<UnityEngine.GameObject>(FindObjectsInactive.Include,
                FindObjectsSortMode.None).Where(go => go.scene == scene);

            foreach (var go in gameObjects)
            {
                ctx.StartRecordingGameObjectInternal(go);
            }
        }

        private void RecordUnloadScene(UnityEngine.SceneManagement.Scene scene, RecorderContext ctx)
        {
            if (!_cachedSceneIdentifiers.TryGetValue(scene, out var sceneIdentifier)) return;

            var unloadSceneSample = new UnloadScene
            {
                Scene = sceneIdentifier
            };

            _unloadSceneSamples.Add(unloadSceneSample);

            var gameObjects = Object.FindObjectsByType<UnityEngine.GameObject>(FindObjectsInactive.Include,
                FindObjectsSortMode.None).Where(go => go.scene == scene);

            foreach (var go in gameObjects)
            {
                ctx.StopRecordingGameObjectInternal(go);
            }
        }

        private void RecordChangeActiveScene(UnityEngine.SceneManagement.Scene scene, RecorderContext ctx)
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