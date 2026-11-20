using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityRuntimeGuid;
using UnityObject = UnityEngine.Object;

namespace PLUME.Core.Object.SafeRef
{
    /// <summary>
    /// Provides a way to create and cache safe reference to unity objects preventing NPE when accessing destroyed objects,
    /// while still being able to access the object identifier (GUID, id).
    /// This is the single source of object identity in a record: objects serialized in a scene keep the GUID stored in
    /// the scene's GUID registry, so that they can be matched across records. Objects created at runtime have no
    /// persistent identity to keep, so they get a random GUID and nothing is added to the registry.
    /// </summary>
    public class SafeRefProvider
    {
        private const int MinEvictionThreshold = 1024;

        private delegate IComponentSafeRef ComponentSafeRefFactory(Component component, Guid guid,
            GameObjectSafeRef gameObjectSafeRef);

        private delegate IAssetSafeRef AssetSafeRefFactory(UnityObject asset, Guid guid, FixedString512Bytes assetPath);

        // Safe refs are generic over the object's runtime type: one factory is built per type, then reused.
        private static readonly Dictionary<Type, ComponentSafeRefFactory> ComponentSafeRefFactories = new();
        private static readonly Dictionary<Type, AssetSafeRefFactory> AssetSafeRefFactories = new();

        private readonly Dictionary<int, IObjectSafeRef> _cachedObjectRefs = new();
        private readonly Dictionary<int, SceneSafeRef> _cachedSceneRefs = new();
        private readonly Dictionary<int, SceneGuidRegistry> _cachedSceneRegistries = new();
        private readonly List<int> _evictedInstanceIds = new();
        private int _evictionThreshold = MinEvictionThreshold;

        public SceneSafeRef GetOrCreateSceneSafeRef(Scene scene)
        {
            if (!scene.IsValid())
            {
                return SceneSafeRef.Null;
            }

            if (_cachedSceneRefs.TryGetValue(scene.handle, out var cachedRef))
                return cachedRef;

            // Unique per scene asset, even for duplicated scenes sharing their registry GUID.
            var sceneGuid = Guid.FromString(
                SceneGuidResolver.Resolve(GetSceneGuidRegistry(scene).SceneGuid, SceneGuidResolver.GetSceneKey(scene)));
            var sceneSafeRef = new SceneSafeRef(scene, sceneGuid, scene.path);
            _cachedSceneRefs[scene.handle] = sceneSafeRef;
            return sceneSafeRef;
        }

        public GameObjectSafeRef GetOrCreateGameObjectSafeRef(GameObject go)
        {
            if (go == null)
                return GameObjectSafeRef.Null;

            var instanceId = go.GetInstanceID();

            if (_cachedObjectRefs.TryGetValue(instanceId, out var cachedRef) && cachedRef is GameObjectSafeRef goRef)
                return goRef;

            if (!go.scene.IsValid())
                return GameObjectSafeRef.Null;

            var sceneGuidRegistry = GetSceneGuidRegistry(go.scene);
            var transform = go.transform;
            var goGuid = GetGuid(sceneGuidRegistry, go);
            var transformGuid = GetGuid(sceneGuidRegistry, transform);
            var sceneSafeRef = GetOrCreateSceneSafeRef(go.scene);
            goRef = new GameObjectSafeRef(go, goGuid, transformGuid, sceneSafeRef);
            Cache(instanceId, goRef);
            return goRef;
        }

        public IComponentSafeRef<TC> GetOrCreateComponentSafeRef<TC>(TC component) where TC : Component
        {
            if (component == null)
                return ComponentSafeRef<TC>.Null;

            var instanceId = component.GetInstanceID();

            if (_cachedObjectRefs.TryGetValue(instanceId, out var cachedRef) &&
                cachedRef is IComponentSafeRef<TC> componentSafeRef)
                return componentSafeRef;

            var go = component.gameObject;
            if (!go.scene.IsValid())
                return ComponentSafeRef<TC>.Null;

            var gameObjectRef = GetOrCreateGameObjectSafeRef(go);
            // A GameObject's identifier embeds its transform's GUID: the transform takes it, which also keeps its
            // identity when Unity replaces it by a RectTransform (e.g. when a UI component is added).
            var guid = component is Transform
                ? gameObjectRef.Identifier.TransformGuid
                : GetGuid(GetSceneGuidRegistry(go.scene), component);
            componentSafeRef = (IComponentSafeRef<TC>)CreateComponentSafeRef(component, guid, gameObjectRef);
            Cache(instanceId, componentSafeRef);
            return componentSafeRef;
        }

        public IAssetSafeRef<TA> GetOrCreateAssetSafeRef<TA>(TA asset) where TA : UnityObject
        {
            if (asset == null)
                return AssetSafeRef<TA>.Null;

            var instanceId = asset.GetInstanceID();

            if (_cachedObjectRefs.TryGetValue(instanceId, out var cachedRef) &&
                cachedRef is IAssetSafeRef<TA> assetSafeRef)
                return assetSafeRef;

            var assetsGuidRegistry = AssetsGuidRegistry.GetOrCreate();
            var assetsGuidRegistryEntry = assetsGuidRegistry.GetOrCreateEntry(asset);
            var assetBundlePath = new FixedString512Bytes(assetsGuidRegistryEntry.assetBundlePath);
            var assetGuid = Guid.FromString(assetsGuidRegistryEntry.guid);
            assetSafeRef = (IAssetSafeRef<TA>)CreateAssetSafeRef(asset, assetGuid, assetBundlePath);
            Cache(instanceId, assetSafeRef);
            return assetSafeRef;
        }

        /// <summary>
        /// Forgets every cached reference, e.g. when a recording ends. Objects serialized in a scene get the same
        /// GUID back when referenced again.
        /// </summary>
        internal void Clear()
        {
            _cachedObjectRefs.Clear();
            _cachedSceneRefs.Clear();
            _cachedSceneRegistries.Clear();
            _evictionThreshold = MinEvictionThreshold;
        }

        private static Guid GetGuid(SceneGuidRegistry sceneGuidRegistry, UnityObject obj)
        {
            return sceneGuidRegistry.TryGetEntry(obj, out var entry) ? Guid.FromString(entry.guid) : Guid.NewRandom();
        }

        private SceneGuidRegistry GetSceneGuidRegistry(Scene scene)
        {
            // A cached registry is destroyed when its scene unloads, and the handle of a scene loaded again changes.
            if (_cachedSceneRegistries.TryGetValue(scene.handle, out var registry) && registry != null)
                return registry;

            registry = SceneGuidRegistry.GetOrCreate(scene);
            _cachedSceneRegistries[scene.handle] = registry;
            return registry;
        }

        /// <summary>
        /// Caches a reference, first evicting the references of destroyed objects whenever the cache has doubled
        /// since the last eviction: the cost stays amortized O(1) per object and the cache does not grow with the
        /// number of objects ever created.
        /// </summary>
        private void Cache(int instanceId, IObjectSafeRef safeRef)
        {
            if (_cachedObjectRefs.Count >= _evictionThreshold)
            {
                foreach (var (cachedInstanceId, cachedRef) in _cachedObjectRefs)
                {
                    // Unity's null check: true once the object is destroyed. Instance ids are never reused.
                    if (cachedRef.Object == null)
                        _evictedInstanceIds.Add(cachedInstanceId);
                }

                foreach (var evictedInstanceId in _evictedInstanceIds)
                    _cachedObjectRefs.Remove(evictedInstanceId);

                _evictedInstanceIds.Clear();
                _evictionThreshold = Math.Max(MinEvictionThreshold, _cachedObjectRefs.Count * 2);
            }

            _cachedObjectRefs[instanceId] = safeRef;
        }

        private static IComponentSafeRef CreateComponentSafeRef(Component component, Guid guid,
            GameObjectSafeRef gameObjectRef)
        {
            var type = component.GetType();

            if (!ComponentSafeRefFactories.TryGetValue(type, out var factory))
            {
                factory = (ComponentSafeRefFactory)typeof(SafeRefProvider)
                    .GetMethod(nameof(NewComponentSafeRef), BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(type)
                    .CreateDelegate(typeof(ComponentSafeRefFactory));
                ComponentSafeRefFactories[type] = factory;
            }

            return factory(component, guid, gameObjectRef);
        }

        private static IAssetSafeRef CreateAssetSafeRef(UnityObject asset, Guid guid, FixedString512Bytes assetPath)
        {
            var type = asset.GetType();

            if (!AssetSafeRefFactories.TryGetValue(type, out var factory))
            {
                factory = (AssetSafeRefFactory)typeof(SafeRefProvider)
                    .GetMethod(nameof(NewAssetSafeRef), BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(type)
                    .CreateDelegate(typeof(AssetSafeRefFactory));
                AssetSafeRefFactories[type] = factory;
            }

            return factory(asset, guid, assetPath);
        }

        private static IComponentSafeRef NewComponentSafeRef<TC>(Component component, Guid guid,
            GameObjectSafeRef gameObjectSafeRef) where TC : Component
        {
            return new ComponentSafeRef<TC>((TC)component, guid, gameObjectSafeRef);
        }

        private static IAssetSafeRef NewAssetSafeRef<TA>(UnityObject asset, Guid guid, FixedString512Bytes assetPath)
            where TA : UnityObject
        {
            return new AssetSafeRef<TA>((TA)asset, guid, assetPath);
        }
    }
}
