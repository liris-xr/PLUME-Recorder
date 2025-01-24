using System.Linq;
using PLUME.Core.Hooks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;
using UnityRuntimeGuid;

namespace PLUME.Base.Hooks
{
    [Preserve]
    public class SceneManagerHooks : IRegisterHooksCallback
    {
        public delegate void OnGameObjectMovedToSceneDelegate(GameObject go, Scene oldScene, Scene scene);
        public static event OnGameObjectMovedToSceneDelegate OnGameObjectMovedToScene = delegate { };

        public void RegisterHooks(HooksRegistry hooksRegistry)
        {
            hooksRegistry.RegisterHook(
                typeof(SceneManagerHooks).GetMethod(nameof(MoveGameObjectToSceneAndNotify),
                    new[] { typeof(GameObject), typeof(Scene) }),
                typeof(SceneManager).GetMethod(nameof(SceneManager.MoveGameObjectToScene),
                    new[] { typeof(GameObject), typeof(Scene) }));
            hooksRegistry.RegisterHook(
                typeof(SceneManagerHooks).GetMethod(nameof(MoveGameObjectsToSceneAndNotify),
                    new[] { typeof(NativeArray<int>), typeof(Scene) }),
                typeof(SceneManager).GetMethod(nameof(SceneManager.MoveGameObjectsToScene),
                    new[] { typeof(NativeArray<int>), typeof(Scene) }));
        }

        internal static void NotifyGameObjectMovedToScene(GameObject go, Scene oldScene, Scene scene)
        {
            OnGameObjectMovedToScene(go, oldScene, scene);
        }
        
        public static void MoveGameObjectToSceneAndNotify(GameObject go, Scene scene)
        {
            var oldScene = go.scene;
            SceneManager.MoveGameObjectToScene(go, scene);
            NotifyGameObjectMovedToScene(go, oldScene, scene);
        }

        public static void MoveGameObjectsToSceneAndNotify(NativeArray<int> instanceIDs, Scene scene)
        {
            var oldScenes = instanceIDs.Select(UnityObjectUtils.FindObjectFromInstanceID<GameObject>)
                .Select(go => go.scene).ToArray();
            
            for(var i = 0; i < instanceIDs.Length; i++)
            {
                var go = UnityObjectUtils.FindObjectFromInstanceID<GameObject>(instanceIDs[i]);
                
                if (go == null)
                    continue;
                
                var oldScene = oldScenes[i];
                SceneManager.MoveGameObjectToScene(go, scene);
                NotifyGameObjectMovedToScene(go, oldScene, scene);
            }
        }
    }
}