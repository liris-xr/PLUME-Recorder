using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace PLUME.Guid
{
    public class SceneObjectsGuidRegistry : MonoBehaviour
    {
        [SerializeField] private GuidRegistry<SceneObjectGuidRegistryEntry> registry = new();

        private static Dictionary<Scene, SceneObjectsGuidRegistry> _instancesMap = new();
        
        public bool TryAdd(SceneObjectGuidRegistryEntry guidEntry)
        {
            return registry.TryAdd(guidEntry);
        }

        public void Clear()
        {
            registry.Clear();
        }

        public Dictionary<Object, SceneObjectGuidRegistryEntry> Copy()
        {
            return registry.Copy();
        }

        public bool TryGetValue(Object obj, out SceneObjectGuidRegistryEntry entry)
        {
            return registry.TryGetValue(obj, out entry);
        }

        public static SceneObjectGuidRegistryEntry CreateNewEntry(Object obj)
        {
            return new SceneObjectGuidRegistryEntry
            {
                @object = obj,
                guid = System.Guid.NewGuid().ToString()
            };
        }
        
        public static SceneObjectsGuidRegistry GetOrCreateInActiveScene()
        {
            return GetOrCreateInScene(SceneManager.GetActiveScene());
        }
        
        public static SceneObjectsGuidRegistry GetOrCreateInScene(Scene scene)
        {
            if (!scene.IsValid())
                throw new Exception("Scene is invalid.");
            if (!scene.isLoaded)
                throw new Exception("Scene is not loaded.");

            if (_instancesMap.TryGetValue(scene, out var sceneObjectsGuidRegistry)) return sceneObjectsGuidRegistry;
            
            foreach (var rootGameObject in scene.GetRootGameObjects())
            {
                sceneObjectsGuidRegistry = rootGameObject.GetComponentInChildren<SceneObjectsGuidRegistry>();

                if (sceneObjectsGuidRegistry != null)
                    break;
            }

            if (sceneObjectsGuidRegistry == null)
            {
                var go = new GameObject("[PLUME] SceneObjectsGuidRegistry", typeof(SceneObjectsGuidRegistry));
                sceneObjectsGuidRegistry = go.GetComponent<SceneObjectsGuidRegistry>();
            }

            _instancesMap[scene] = sceneObjectsGuidRegistry;

            return sceneObjectsGuidRegistry;
        }

        public SceneObjectGuidRegistryEntry GetOrCreate(Object obj)
        {
            if (registry.TryGetValue(obj, out var sceneObjectGuidRegistryEntry)) return sceneObjectGuidRegistryEntry;
            sceneObjectGuidRegistryEntry = CreateNewEntry(obj);
            registry.TryAdd(sceneObjectGuidRegistryEntry);
            return sceneObjectGuidRegistryEntry;
        }
    }
}