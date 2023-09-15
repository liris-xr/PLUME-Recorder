using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PLUME.Guid
{
    [Serializable]
    public class AssetsGuidRegistry : ScriptableObject
    {
        public const string AssetPath =
            "Packages/fr.liris.plume.recorder/ScriptableObjects/AssetsGuidRegistry.asset";

        [SerializeField] private GuidRegistry<AssetGuidRegistryEntry> registry = new();

        public int Count => registry.Count;

        public bool TryAdd(AssetGuidRegistryEntry guidEntry)
        {
            return registry.TryAdd(guidEntry);
        }

        public void Clear()
        {
            registry.Clear();
        }

        public Dictionary<Object, AssetGuidRegistryEntry> Copy()
        {
            return registry.Copy();
        }

        public bool TryGetValue(Object obj, out AssetGuidRegistryEntry entry)
        {
            return registry.TryGetValue(obj, out entry);
        }

        public AssetGuidRegistryEntry GetOrCreate(Object obj)
        {
            if (registry.TryGetValue(obj, out var assetGuidRegistryEntry)) return assetGuidRegistryEntry;
            assetGuidRegistryEntry = CreateNewEntry(obj);
            registry.TryAdd(assetGuidRegistryEntry);
            return assetGuidRegistryEntry;
        }
        
        public static AssetGuidRegistryEntry CreateNewEntry(Object obj)
        {
            return new AssetGuidRegistryEntry
            {
                @object = obj,
                guid = System.Guid.NewGuid().ToString(),
#if UNITY_EDITOR
                assetBundlePath = GetFullAssetBundlePath(obj)
#else
                assetBundlePath = ""
#endif
            };
        }

#if UNITY_EDITOR
        private static string GetFullAssetBundlePath(Object asset)
        {
            const string builtinResourcesPath = "Resources/unity_builtin_extra";
            const string builtinExtraResourcesPath = "Library/unity default resources";

            if (!AssetDatabase.Contains(asset))
                return null;

            var path = AssetDatabase.GetAssetPath(asset);

            // TODO: differentiate two assets of same type inside a .obj or .fbx (ex: .obj containing two Material)
            if (path.Equals(builtinResourcesPath) || path.Equals(builtinExtraResourcesPath))
                return $"Builtin:{asset.GetType().Name}:{path}/{asset.name}";

            return $"Custom:{asset.GetType().Name}:{path}";
        }
#endif
    }
}