using System;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace PLUME
{
    public readonly struct ObjectSafeRef<TObject> : IObjectSafeRef where TObject : UnityObject
    {
        public readonly ObjectIdentifier ObjectIdentifier;
        public readonly TObject TypedObject;

        public static ObjectSafeRef<TObject> Null { get; } = new(ObjectIdentifier.Null);

        private ObjectSafeRef(ObjectIdentifier identifier)
        {
            ObjectIdentifier = identifier;
            TypedObject = null;
        }

        public ObjectSafeRef(TObject @object, Hash128 guid)
        {
            ObjectIdentifier = new ObjectIdentifier(@object.GetInstanceID(), guid);
            TypedObject = @object;
        }

        public bool Equals(ObjectSafeRef<TObject> other)
        {
            return ObjectIdentifier.InstanceId == other.ObjectIdentifier.InstanceId;
        }

        public override bool Equals(object obj)
        {
            return obj is ObjectSafeRef<TObject> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return ObjectIdentifier.InstanceId;
        }

        public Type GetObjectType()
        {
            return typeof(TObject);
        }

        public ObjectIdentifier GetObjectIdentifier()
        {
            return ObjectIdentifier;
        }

        public UnityObject GetObject()
        {
            return TypedObject;
        }

        public int GetInstanceId()
        {
            return ObjectIdentifier.InstanceId;
        }

        public Hash128 GetGlobalId()
        {
            return ObjectIdentifier.GlobalId;
        }

        public override string ToString()
        {
            return (TypedObject == null ? "null" : TypedObject.name) + $" ({typeof(TObject)}): {ObjectIdentifier}";
        }
    }
}