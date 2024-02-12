using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityRuntimeGuid;
using UnityObject = UnityEngine.Object;

namespace PLUME.Core.Object.SafeRef
{
    public class ObjectSafeRefProvider
    {
        private readonly Dictionary<int, IObjectSafeRef> _cachedRefs = new();

        public ObjectSafeRef<TObject> GetOrCreateTypedObjectSafeRef<TObject>(TObject obj) where TObject : UnityObject
        {
            return (ObjectSafeRef<TObject>) GetOrCreateObjectSafeRef(obj);
        }

        public IObjectSafeRef GetOrCreateObjectSafeRef(UnityObject obj)
        {
            if (obj == null)
                return null;

            var instanceId = obj.GetInstanceID();

            if (_cachedRefs.TryGetValue(instanceId, out var cachedRef) && cachedRef.GetObjectType() == obj.GetType())
            {
                return cachedRef;
            }

            var guidRegistryEntry = GetGuidRegistryEntry(obj);
            var guid = Hash128.Parse(guidRegistryEntry.guid);
            
            var objRefType = typeof(ObjectSafeRef<>).MakeGenericType(obj.GetType());
            var objRefCtor = objRefType.GetConstructor(new[] { obj.GetType(), typeof(Hash128) });
            
            if (objRefCtor == null)
                throw new MissingMethodException($"Constructor with parameters ({obj.GetType()}, {typeof(Hash128)}) not found on type {objRefType}");
            
            var objRef = (IObjectSafeRef) objRefCtor!.Invoke(new object[] { obj, guid });
            
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