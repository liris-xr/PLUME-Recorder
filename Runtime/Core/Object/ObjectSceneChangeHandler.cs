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

        public static void OnGameObjectMovedToScene(GameObject go, Scene oldScene, Scene scene)
        {
            // Sanity check
            if (!oldScene.IsValid() || !scene.IsValid())
                return;
            
            // Move the game object from one registry to another to keep its GUID
            var oldSceneGuidRegistry = SceneGuidRegistry.GetOrCreate(oldScene);
            var newSceneGuidRegistry = SceneGuidRegistry.GetOrCreate(scene);

            var components = ListPool<Component>.Get();
            go.GetComponentsInChildren(true, components);

            var goRegistryEntry = oldSceneGuidRegistry.GetOrCreateEntry(go);
            oldSceneGuidRegistry.Remove(go);
            newSceneGuidRegistry.TryAdd(goRegistryEntry);
            
            foreach (var component in components)
            {
                var componentRegistryEntry = oldSceneGuidRegistry.GetOrCreateEntry(component);
                oldSceneGuidRegistry.Remove(component);
                newSceneGuidRegistry.TryAdd(componentRegistryEntry);
            }
        }
    }
}