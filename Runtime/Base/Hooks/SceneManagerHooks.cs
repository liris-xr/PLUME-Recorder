using System;
using PLUME.Core;
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
            try
            {
                OnGameObjectMovedToScene(go, oldScene, scene);
            }
            catch (Exception e)
            {
                ErrorReporter.Report($"{nameof(SceneManagerHooks)}.{nameof(OnGameObjectMovedToScene)}",
                    "Failed to record a GameObject moved to another scene. The application is not affected.", e);
            }
        }
        
        public static void MoveGameObjectToSceneAndNotify(GameObject go, Scene scene)
        {
            // A null or destroyed GameObject is left to the user's call, so that Unity raises its own exception.
            if (go == null)
            {
                SceneManager.MoveGameObjectToScene(go, scene);
                return;
            }

            var oldScene = go.scene;
            SceneManager.MoveGameObjectToScene(go, scene);
            NotifyGameObjectMovedToScene(go, oldScene, scene);
        }

        public static void MoveGameObjectsToSceneAndNotify(NativeArray<int> instanceIDs, Scene scene)
        {
            // Unknown instance IDs are skipped here and left for Unity to handle, so that the user's move behaves
            // exactly as the original call. A disposed array is not read at all, so that Unity's own check reports it.
            if (!instanceIDs.IsCreated)
            {
                SceneManager.MoveGameObjectsToScene(instanceIDs, scene);
                return;
            }

            var oldScenes = new Scene[instanceIDs.Length];

            for (var i = 0; i < instanceIDs.Length; i++)
            {
                var go = UnityObjectUtils.FindObjectFromInstanceID<GameObject>(instanceIDs[i]);

                if (go != null)
                    oldScenes[i] = go.scene;
            }

            SceneManager.MoveGameObjectsToScene(instanceIDs, scene);

            for (var i = 0; i < instanceIDs.Length; i++)
            {
                var go = UnityObjectUtils.FindObjectFromInstanceID<GameObject>(instanceIDs[i]);

                if (go != null)
                    NotifyGameObjectMovedToScene(go, oldScenes[i], scene);
            }
        }
    }
}