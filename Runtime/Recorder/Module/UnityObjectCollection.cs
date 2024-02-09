using System.Collections.Generic;
using UnityEngine;

namespace PLUME.Recorder.Module
{
    public class UnityObjectCollection<TObject> where TObject : Object
    {
        private readonly HashSet<ObjectIdentifier> _objectsIdentifiers = new(ObjectIdentifierComparer.Instance);
        private readonly List<ObjectSafeRef<TObject>> _objects = new();

        public bool TryAdd(ObjectSafeRef<TObject> objSafeRef)
        {
            if (!_objectsIdentifiers.Add(objSafeRef.ObjectIdentifier)) return false;
            _objects.Add(objSafeRef);
            return true;
        }
        
        public bool TryRemove(ObjectSafeRef<TObject> objSafeRef)
        {
            if (!_objectsIdentifiers.Remove(objSafeRef.ObjectIdentifier)) return false;
            _objects.Remove(objSafeRef);
            return true;
        }
        
        public bool Contains(ObjectSafeRef<TObject> objSafeRef)
        {
            return _objectsIdentifiers.Contains(objSafeRef.ObjectIdentifier);
        }
        
        public void Clear()
        {
            _objectsIdentifiers.Clear();
            _objects.Clear();
        }
        
        public IReadOnlyList<ObjectSafeRef<TObject>> AsReadOnly()
        {
            return _objects;
        }
    }
}