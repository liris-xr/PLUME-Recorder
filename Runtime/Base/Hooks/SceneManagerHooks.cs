using PLUME.Core.Hooks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;

namespace PLUME.Base.Hooks
{
    [Preserve]
    public class SceneManagerHooks : IRegisterHooksCallback
    {
        public delegate void OnMoveGameObjectToSceneDelegate(GameObject go, Scene scene);

        public static event OnMoveGameObjectToSceneDelegate OnMoveGameObjectToScene = delegate { };

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

        public static void MoveGameObjectToSceneAndNotify(GameObject go, Scene scene)
        {
            SceneManager.MoveGameObjectToScene(go, scene);
            OnMoveGameObjectToScene(go, scene);
        }

        public static void MoveGameObjectsToSceneAndNotify(NativeArray<int> instanceIDs, Scene scene)
        {
            SceneManager.MoveGameObjectsToScene(instanceIDs, scene);
            foreach (var instanceId in instanceIDs)
            {
                var go = UnityObjectUtils.FindObjectFromInstanceID<GameObject>(instanceId);

                if (go != null)
                {
                    OnMoveGameObjectToScene(go, scene);
                }
            }
        }
    }
}