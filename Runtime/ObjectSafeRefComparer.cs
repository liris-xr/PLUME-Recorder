using System.Collections.Generic;
using UnityObject = UnityEngine.Object;

namespace PLUME
{
    public sealed class ObjectSafeRefComparer<TObject> : IEqualityComparer<ObjectSafeRef<TObject>> where TObject : UnityObject
    {
        public static ObjectSafeRefComparer<TObject> Instance { get; } = new();
        
        public bool Equals(ObjectSafeRef<TObject> x, ObjectSafeRef<TObject> y)
        {
            return ObjectIdentifierComparer.Instance.Equals(x.ObjectIdentifier, y.ObjectIdentifier);
        }

        public int GetHashCode(ObjectSafeRef<TObject> obj)
        {
            return ObjectIdentifierComparer.Instance.GetHashCode(obj.ObjectIdentifier);
        }
    }
}