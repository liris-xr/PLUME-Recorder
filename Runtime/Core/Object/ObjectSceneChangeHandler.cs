using System;
using PLUME.Base.Hooks;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using UnityRuntimeGuid;

namespace PLUME.Core.Object
{
    // TODO: This should probably be handled in the Unity-Runtime-Guid package, but this would require to externalize the hooks system
    public class ObjectSceneChangeHandler
    {
        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            SceneManagerHooks.OnGameObjectMovedToScene += OnGameObjectMovedToScene;

            Application.quitting += () =>
            {
                SceneManagerHooks.OnGameObjectMovedToScene -= OnGameObjectMovedToScene;
            };
        }

        private static void MoveEntry(UnityEngine.Object obj, SceneGuidRegistry from, SceneGuidRegistry to)
        {
            if (!from.TryGetEntry(obj, out var entry))
                return;

            from.Remove(obj);
            to.TryAdd(entry);
        }

        public static void OnGameObjectMovedToScene(GameObject go, Scene oldScene, Scene scene)
        {
            // Sanity check
            if (!oldScene.IsValid() || !scene.IsValid())
                return;

            // Called from the SceneManager.MoveGameObjectToScene hook: an exception escaping it would break the
            // application's call.
            try
            {
                // Move the game object from one registry to another to keep its GUID
                var oldSceneGuidRegistry = SceneGuidRegistry.GetOrCreate(oldScene);
                var newSceneGuidRegistry = SceneGuidRegistry.GetOrCreate(scene);

                using var _ = ListPool<Component>.Get(out var components);
                go.GetComponentsInChildren(true, components);

                // Only objects serialized in a scene have a registry entry: runtime objects have no GUID to move.
                MoveEntry(go, oldSceneGuidRegistry, newSceneGuidRegistry);

                foreach (var component in components)
                    MoveEntry(component, oldSceneGuidRegistry, newSceneGuidRegistry);
            }
            catch (Exception e)
            {
                ErrorReporter.Report($"{nameof(ObjectSceneChangeHandler)}.{nameof(OnGameObjectMovedToScene)}",
                    "Failed to move a GameObject's GUIDs to its new scene. Its identifiers may change in the record.", e);
            }
        }
    }
}