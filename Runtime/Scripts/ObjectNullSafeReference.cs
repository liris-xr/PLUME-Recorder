using UnityEngine;

namespace Runtime
{
    public class ObjectNullSafeReference<T> where T : Object
    {
        public readonly int InstanceId;
        public readonly T Object;

        public ObjectNullSafeReference(T obj)
        {
            Object = obj;
            InstanceId = obj.GetInstanceID();
        }

        public override bool Equals(object obj)
        {
            return obj is ObjectNullSafeReference<T> objRef && InstanceId.Equals(objRef.InstanceId);
        }

        public override int GetHashCode()
        {
            return InstanceId;
        }

        public bool HasBeenDestroyed()
        {
            return Object == null || ReferenceEquals(Object, null);
        }
    }
}