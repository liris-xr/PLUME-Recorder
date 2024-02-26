using System.Collections.Generic;
using PLUME.Sample.ProtoBurst;
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

        public ObjectSafeRef<TObject> GetOrCreateTypedObjectSafeRef<TObject>(TObject obj) where TObject : UnityObject
        {
            return (ObjectSafeRef<TObject>)GetOrCreateObjectSafeRef(obj);
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

            if (_cachedRefs.TryGetValue(instanceId, out var cachedRef) && cachedRef.ObjectType == obj.GetType())
            {
                return cachedRef;
            }

            IObjectSafeRef objRef;

            if (obj is GameObject go && go.scene.IsValid())
            {
                var sceneGuidRegistry = SceneGuidRegistry.GetOrCreate(go.scene);
                var sceneGuidRegistryEntry = sceneGuidRegistry.GetOrCreateEntry(obj);
                objRef = CreateSceneObjectSafeRef(obj, sceneGuidRegistryEntry);
            }
            else if (obj is Component component && component.gameObject.scene.IsValid())
            {
                var sceneGuidRegistry = SceneGuidRegistry.GetOrCreate(component.gameObject.scene);
                var sceneGuidRegistryEntry = sceneGuidRegistry.GetOrCreateEntry(obj);
                objRef = CreateSceneObjectSafeRef(obj, sceneGuidRegistryEntry);
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

        private static IObjectSafeRef CreateSceneObjectSafeRef(UnityObject obj, SceneGuidRegistryEntry guidEntry)
        {
            var objRefType = typeof(SceneObjectSafeRef<>).MakeGenericType(obj.GetType());
            var objRefCtor = objRefType.GetConstructor(new[] { obj.GetType(), typeof(Guid) });
            return (IObjectSafeRef)objRefCtor!.Invoke(new object[] { obj, new Guid(guidEntry.guid) });
        }

        private static IObjectSafeRef CreateAssetObjectSafeRef(UnityObject obj, AssetGuidRegistryEntry guidEntry)
        {
            var objRefType = typeof(AssetObjectSafeRef<>).MakeGenericType(obj.GetType());
            var objRefCtor =
                objRefType.GetConstructor(new[] { obj.GetType(), typeof(Guid), typeof(FixedString512Bytes) });
            return (IObjectSafeRef)objRefCtor!.Invoke(new object[]
            {
                obj, new Guid(guidEntry.guid),
                new FixedString512Bytes(guidEntry.assetBundlePath)
            });
        }
    }
}