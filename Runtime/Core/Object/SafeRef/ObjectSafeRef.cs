using System;
using UnityEngine;
using Guid = PLUME.Sample.ProtoBurst.Guid;
using UnityObject = UnityEngine.Object;

namespace PLUME.Core.Object.SafeRef
{
    /// <summary>
    /// Stores a reference to a Unity object along with its cached <see cref="Identifier"/>. When the object is
    /// destroyed, the identifier is still valid and can be used to identify the object. This is useful for recording
    /// data about objects that have been destroyed and for which the reference is no longer valid.
    /// </summary>
    /// <typeparam name="TObject">The type of the Unity object to reference.</typeparam>
    public readonly struct ObjectSafeRef<TObject> : IObjectSafeRef where TObject : UnityObject
    {
        public static ObjectSafeRef<TObject> Null { get; } = new(ObjectIdentifier.Null);
        
        public Type ObjectType => typeof(TObject);

        public ObjectIdentifier Identifier { get; }

        public readonly TObject TypedObject;

        public UnityObject Object => TypedObject;

        private ObjectSafeRef(ObjectIdentifier identifier)
        {
            Identifier = identifier;
            TypedObject = null;
        }

        public ObjectSafeRef(TObject @object, Guid guid)
        {
            Identifier = new ObjectIdentifier(@object.GetInstanceID(), guid);
            TypedObject = @object;
        }

        public bool Equals(ObjectSafeRef<TObject> other)
        {
            return Identifier.InstanceId == other.Identifier.InstanceId;
        }

        public override bool Equals(object obj)
        {
            return obj is ObjectSafeRef<TObject> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Identifier.InstanceId;
        }

        public override string ToString()
        {
            return (TypedObject == null ? "null" : TypedObject.name) + $" ({typeof(TObject)}): {Identifier}";
        }
    }
}