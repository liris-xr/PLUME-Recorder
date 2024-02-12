using System.Collections.Generic;
using PLUME.Core.Object.SafeRef;

namespace PLUME.Core.Object
{
    public class ObjectCollection<TObject> where TObject : UnityEngine.Object
    {
        private readonly HashSet<ObjectIdentifier> _objectsIdentifiers = new(ObjectIdentifierComparer.Instance);
        private readonly List<ObjectSafeRef<TObject>> _objects = new();

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
        
        public IReadOnlyList<ObjectSafeRef<TObject>> AsReadOnly()
        {
            return _objects;
        }
    }
}