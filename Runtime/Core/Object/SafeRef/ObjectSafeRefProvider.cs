using System.Collections.Generic;
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

            if (GetOrCreateObjectSafeRef(go) is GameObjectSafeRef goRef)
            {
                return goRef;
            }

            return GameObjectSafeRef.Null;
        }

        public ComponentSafeRef<TC> GetOrCreateComponentSafeRef<TC>(TC component) where TC : Component
        {
            if (component == null)
                return ComponentSafeRef<TC>.Null;

            if (GetOrCreateObjectSafeRef(component) is ComponentSafeRef<TC> componentSafeRef)
            {
                return componentSafeRef;
            }

            return ComponentSafeRef<TC>.Null;
        }

        public AssetSafeRef<TA> GetOrCreateAssetSafeRef<TA>(TA asset) where TA : UnityObject
        {
            if (asset == null)
                return AssetSafeRef<TA>.Null;

            if (GetOrCreateObjectSafeRef(asset) is AssetSafeRef<TA> assetSafeRef)
            {
                return assetSafeRef;
            }

            return AssetSafeRef<TA>.Null;
        }

        /// <summary>
        /// Returns an <see cref="ObjectSafeRef{TObject}"/> as an <see cref="IObjectSafeRef"/> instance for the given Unity object.
        /// Uses a cache to avoid having to query the GUID registries every time this function is called.
        /// When called from a script for an object contained a scene, make sure that the scene is loaded before calling
        /// this method, otherwise this method will throw an exception.
        /// </summary>
        /// <param name="obj">The Unity object for which to get the safe reference.</param>
        /// <returns>An instance of <see cref="ObjectSafeRef{TObject}"/> for the given object.</returns>
        public IObjectSafeRef GetOrCreateObjectSafeRef(UnityObject obj)
        {
            if (obj == null)
                return null;

            var instanceId = obj.GetInstanceID();

            if (_cachedRefs.TryGetValue(instanceId, out var cachedRef))
            {
                return cachedRef;
            }

            IObjectSafeRef objRef;

            if (obj is GameObject go && go.scene.IsValid())
            {
                var sceneGuidRegistry = SceneGuidRegistry.GetOrCreate(go.scene);
                var goRegistryEntry = sceneGuidRegistry.GetOrCreateEntry(go);
                var transformRegistryEntry = sceneGuidRegistry.GetOrCreateEntry(go.transform);
                objRef = CreateGameObjectSafeRef(go, goRegistryEntry, transformRegistryEntry);
            }
            else if (obj is Component component && component.gameObject.scene.IsValid())
            {
                var sceneGuidRegistry = SceneGuidRegistry.GetOrCreate(component.gameObject.scene);
                var guidRegistryEntry = sceneGuidRegistry.GetOrCreateEntry(obj);
                var gameObjectRef = GetOrCreateGameObjectSafeRef(component.gameObject);
                objRef = CreateComponentSafeRef(component, gameObjectRef, guidRegistryEntry);
            }
            else
            {
                var assetsGuidRegistry = AssetsGuidRegistry.GetOrCreate();
                var assetsGuidRegistryEntry = assetsGuidRegistry.GetOrCreateEntry(obj);
                objRef = CreateAssetObjectSafeRef(obj, assetsGuidRegistryEntry);
            }

            _cachedRefs[instanceId] = objRef;
            return objRef;
        }

        private static GameObjectSafeRef CreateGameObjectSafeRef(GameObject go, GuidRegistryEntry goGuid,
            GuidRegistryEntry tGuid)
        {
            var goRef = new GameObjectSafeRef(go, new Guid(goGuid.guid), ComponentSafeRef<Transform>.Null);
            var tRef = new ComponentSafeRef<Transform>(go.transform, new Guid(tGuid.guid), GameObjectSafeRef.Null);
            goRef.TransformSafeRef = tRef;
            return goRef;
        }

        private static IObjectSafeRef CreateComponentSafeRef(Component component, GameObjectSafeRef gameObjectRef,
            GuidRegistryEntry guidEntry)
        {
            var type = typeof(ComponentSafeRef<>).MakeGenericType(component.GetType());
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            var parametersType = new[] { component.GetType(), typeof(Guid), typeof(GameObjectSafeRef) };
            var parameters = new object[] { component, new Guid(guidEntry.guid), gameObjectRef };
            var objRefCtor = type.GetConstructor(flags, null, parametersType, null);
            return (IObjectSafeRef)objRefCtor!.Invoke(parameters);
        }

        private static IObjectSafeRef CreateAssetObjectSafeRef(UnityObject obj, AssetGuidRegistryEntry guidEntry)
        {
            var type = typeof(AssetSafeRef<>).MakeGenericType(obj.GetType());
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            var parametersType = new[] { obj.GetType(), typeof(Guid), typeof(FixedString512Bytes) };
            var parameters = new object[]
                { obj, new Guid(guidEntry.guid), new FixedString512Bytes(guidEntry.assetBundlePath) };

            var ctor = type.GetConstructor(flags, null, parametersType, null);
            return (IObjectSafeRef)ctor!.Invoke(parameters);
        }
    }
}