using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

            var guidRegistryEntry = GetGuidRegistryEntry(obj);

            var objRefType = typeof(ObjectSafeRef<>).MakeGenericType(obj.GetType());
            var objRefCtor = objRefType.GetConstructor(new[] { obj.GetType(), typeof(Guid) });
            var objRef = (IObjectSafeRef)objRefCtor!.Invoke(new object[] { obj, new Guid(guidRegistryEntry.guid) });

            _cachedRefs[instanceId] = objRef;
            return objRef;
        }

        [SuppressMessage("ReSharper", "ConvertIfStatementToSwitchStatement")]
        private static GuidRegistryEntry GetGuidRegistryEntry<TObject>(TObject obj) where TObject : UnityObject
        {
            if (obj is GameObject go && go.scene.IsValid())
            {
                var sceneGuidRegistry = SceneGuidRegistry.GetOrCreate(go.scene);
                return sceneGuidRegistry.GetOrCreateEntry(obj);
            }

            if (obj is Component component && component.gameObject.scene.IsValid())
            {
                var sceneGuidRegistry = SceneGuidRegistry.GetOrCreate(component.gameObject.scene);
                return sceneGuidRegistry.GetOrCreateEntry(obj);
            }

            var assetsGuidRegistry = AssetsGuidRegistry.GetOrCreate();
            return assetsGuidRegistry.GetOrCreateEntry(obj);
        }
    }
}