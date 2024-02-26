using System;
using System.Collections.Generic;
using PLUME.Core.Object.SafeRef;
using Unity.Collections;

namespace PLUME.Core.Object
{
    public class ObjectCollection<TObject> : IDisposable where TObject : UnityEngine.Object
    {
        private NativeHashSet<ObjectIdentifier> _objectsIdentifiers;
        private readonly List<ObjectSafeRef<TObject>> _objects = new();

        public ObjectCollection(int initialCapacity, Allocator allocator)
        {
            _objectsIdentifiers = new NativeHashSet<ObjectIdentifier>(initialCapacity, allocator);
        }

        public void Dispose()
        {
            _objectsIdentifiers.Dispose();
        }

        public bool TryAdd(ObjectSafeRef<TObject> objSafeRef)
        {
            if (!_objectsIdentifiers.Add(objSafeRef.Identifier))
                return false;

            _objects.Add(objSafeRef);
            return true;
        }

        public bool TryRemove(ObjectSafeRef<TObject> objSafeRef)
        {
            if (!_objectsIdentifiers.Remove(objSafeRef.Identifier))
                return false;

            _objects.Remove(objSafeRef);
            return true;
        }

        public bool Contains(ObjectSafeRef<TObject> objSafeRef)
        {
            return _objectsIdentifiers.Contains(objSafeRef.Identifier);
        }

        public void Clear()
        {
            _objectsIdentifiers.Clear();
            _objects.Clear();
        }

        public NativeHashSet<ObjectIdentifier>.ReadOnly GetIdentifiers()
        {
            return _objectsIdentifiers.AsReadOnly();
        }

        public IReadOnlyList<ObjectSafeRef<TObject>> AsReadOnly()
        {
            return _objects;
        }
    }
}