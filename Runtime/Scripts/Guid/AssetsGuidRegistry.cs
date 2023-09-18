using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PLUME.Guid
{
    [Serializable]
    public class AssetsGuidRegistry : ScriptableObject
    {
        private const string RegistryResourcePath = "PlumeAssetsGuidRegistry.asset";

        [SerializeField] private GuidRegistry<AssetGuidRegistryEntry> registry = new();

        private static bool _loaded;
        private static AssetsGuidRegistry _loadedInstance;

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

            var prefix = path.Equals(builtinResourcesPath) || path.Equals(builtinExtraResourcesPath)
                ? "Builtin"
                : "Custom";

            return $"{prefix}:{asset.GetType().FullName}:{path}:{asset.name}";
        }
#endif

        public static AssetsGuidRegistry Get()
        {
            if (_loaded && _loadedInstance != null)
                return _loadedInstance;

#if UNITY_EDITOR
            var assetPath = Path.Join("Assets/Resources", RegistryResourcePath);
            var asset = AssetDatabase.LoadAssetAtPath<AssetsGuidRegistry>(assetPath);
            
            if (asset == null)
            {
                Directory.CreateDirectory(Path.Join(Application.dataPath, "Resources"));
                asset = CreateInstance<AssetsGuidRegistry>();
                AssetDatabase.CreateAsset(asset, assetPath);
                Debug.Log($"Created Assets GUID registry at '{assetPath}'");
            }
#endif

            asset = Resources.Load<AssetsGuidRegistry>(RegistryResourcePath.Replace(".asset", ""));

            if (asset == null)
            {
                throw new Exception($"Assets GUID registry not found at 'Assets/Resources/{RegistryResourcePath}'");
            }
            
            _loaded = true;
            _loadedInstance = asset;
            return _loadedInstance;
        }
    }
}