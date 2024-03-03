using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Unity.Collections;
using UnityEngine;
using UnityRuntimeGuid;
using UnityObject = UnityEngine.Object;

namespace PLUME.Core.Object.SafeRef
{
    /// <summary>
    /// Provides a way to create and cache <see cref="IObjectSafeRef"/> instances for Unity objects.
    /// </summary>
    public class ObjectSafeRefProvider
    {
        private readonly Dictionary<int, IObjectSafeRef> _cachedRefs = new();

        public GameObjectSafeRef GetOrCreateGameObjectSafeRef(GameObject go)
        {
            if (go == null)
                return GameObjectSafeRef.Null;

            var instanceId = go.GetInstanceID();

            if (_cachedRefs.TryGetValue(instanceId, out var cachedRef) && cachedRef is GameObjectSafeRef goRef)
                return goRef;

            if (!go.scene.IsValid())
                return GameObjectSafeRef.Null;

            var sceneGuidRegistry = SceneGuidRegistry.GetOrCreate(go.scene);
            var goRegistryEntry = sceneGuidRegistry.GetOrCreateEntry(go);
            var transformRegistryEntry = sceneGuidRegistry.GetOrCreateEntry(go.transform);
            var guidStr = goRegistryEntry.guid.Replace("-", "");
            var goGuid = new Guid(guidStr);
            var transformGuid = new Guid(transformRegistryEntry.guid);
            goRef = new GameObjectSafeRef(go, goGuid, transformGuid);
            _cachedRefs[instanceId] = goRef;
            return goRef;
        }

        public IComponentSafeRef<TC> GetOrCreateComponentSafeRef<TC>(TC component) where TC : Component
        {
            if (component == null)
                return ComponentSafeRef<TC>.Null;

            var instanceId = component.GetInstanceID();

            if (_cachedRefs.TryGetValue(instanceId, out var cachedRef) &&
                cachedRef is IComponentSafeRef<TC> componentSafeRef)
                return componentSafeRef;

            if (!component.gameObject.scene.IsValid())
                return ComponentSafeRef<TC>.Null;

            var sceneGuidRegistry = SceneGuidRegistry.GetOrCreate(component.gameObject.scene);
            var guidRegistryEntry = sceneGuidRegistry.GetOrCreateEntry(component);
            var gameObjectRef = GetOrCreateGameObjectSafeRef(component.gameObject);
            var guidStr = guidRegistryEntry.guid.Replace("-", "");
            var goGuid = new Guid(guidStr);
            componentSafeRef = CreateComponentSafeRef(component, gameObjectRef, goGuid);
            _cachedRefs[instanceId] = componentSafeRef;
            return componentSafeRef;
        }

        public IAssetSafeRef<TA> GetOrCreateAssetSafeRef<TA>(TA asset) where TA : UnityObject
        {
            if (asset == null)
                return AssetSafeRef<TA>.Null;

            var instanceId = asset.GetInstanceID();

            if (_cachedRefs.TryGetValue(instanceId, out var cachedRef) && cachedRef is IAssetSafeRef<TA> assetSafeRef)
                return assetSafeRef;

            var assetsGuidRegistry = AssetsGuidRegistry.GetOrCreate();
            var assetsGuidRegistryEntry = assetsGuidRegistry.GetOrCreateEntry(asset);
            var assetBundlePath = new FixedString512Bytes(assetsGuidRegistryEntry.assetBundlePath);
            var guidStr = assetsGuidRegistryEntry.guid.Replace("-", "");
            var assetGuid = new Guid(guidStr);
            assetSafeRef = CreateAssetObjectSafeRef(asset, assetGuid, assetBundlePath);
            _cachedRefs[instanceId] = assetSafeRef;
            return assetSafeRef;
        }

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