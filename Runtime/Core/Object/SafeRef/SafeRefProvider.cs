using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// </summary>
    public class SafeRefProvider
    {
        private readonly Dictionary<int, IObjectSafeRef> _cachedObjectRefs = new();
        private readonly Dictionary<string, SceneSafeRef> _cachedSceneRefs = new();

        public SceneSafeRef GetOrCreateSceneSafeRef(Scene scene)
        {
            if (!scene.IsValid())
                return SceneSafeRef.Null;

            var sceneGuidRegistry = SceneGuidRegistry.GetOrCreate(scene);
            var sceneGuid = Guid.FromString(sceneGuidRegistry.SceneGuid);

            if (_cachedSceneRefs.TryGetValue(sceneGuidRegistry.SceneGuid, out var cachedRef))
                return cachedRef;
            
            var sceneSafeRef = new SceneSafeRef(scene, sceneGuid, scene.path);
            _cachedSceneRefs[sceneGuidRegistry.SceneGuid] = sceneSafeRef;
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

            var sceneGuidRegistry = SceneGuidRegistry.GetOrCreate(go.scene);
            var goRegistryEntry = sceneGuidRegistry.GetOrCreateEntry(go);
            var transformRegistryEntry = sceneGuidRegistry.GetOrCreateEntry(go.transform);
            var goGuid = Guid.FromString(goRegistryEntry.guid);
            var transformGuid = Guid.FromString(transformRegistryEntry.guid);
            var sceneSafeRef = GetOrCreateSceneSafeRef(go.scene);
            goRef = new GameObjectSafeRef(go, goGuid, transformGuid, sceneSafeRef);
            _cachedObjectRefs[instanceId] = goRef;
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

            if (!component.gameObject.scene.IsValid())
                return ComponentSafeRef<TC>.Null;

            var sceneGuidRegistry = SceneGuidRegistry.GetOrCreate(component.gameObject.scene);
            var guidRegistryEntry = sceneGuidRegistry.GetOrCreateEntry(component);
            var gameObjectRef = GetOrCreateGameObjectSafeRef(component.gameObject);
            var goGuid = Guid.FromString(guidRegistryEntry.guid);
            componentSafeRef = CreateComponentSafeRef(component, gameObjectRef, goGuid);
            _cachedObjectRefs[instanceId] = componentSafeRef;
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
            assetSafeRef = CreateAssetObjectSafeRef(asset, assetGuid, assetBundlePath);
            _cachedObjectRefs[instanceId] = assetSafeRef;
            return assetSafeRef;
        }

        // TODO: Get rid of this
        private static IComponentSafeRef<TC> CreateComponentSafeRef<TC>(TC component,
            GameObjectSafeRef gameObjectRef,
            Guid guid) where TC : Component
        {
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            var type = typeof(ComponentSafeRef<>).MakeGenericType(component.GetType());
            var parameters = new object[] { component, guid, gameObjectRef };
            return (IComponentSafeRef<TC>)Activator.CreateInstance(type, flags, null, parameters,
                CultureInfo.InvariantCulture);
        }

        // TODO: Get rid of this
        private static IAssetSafeRef<TA> CreateAssetObjectSafeRef<TA>(TA obj, Guid guid,
            FixedString512Bytes assetPath) where TA : UnityObject
        {
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            var type = typeof(AssetSafeRef<>).MakeGenericType(obj.GetType());
            var parameters = new object[] { obj, guid, assetPath };
            return (IAssetSafeRef<TA>)Activator.CreateInstance(type, flags, null, parameters,
                CultureInfo.InvariantCulture);
        }
    }
}