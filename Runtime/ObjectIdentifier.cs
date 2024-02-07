using System;
using UnityEngine;

namespace PLUME
{
    public readonly struct ObjectIdentifier : IEquatable<ObjectIdentifier>
    {
        public static ObjectIdentifier Null { get; } = new(0, new Hash128(0, 0, 0, 0));

        public readonly int InstanceId;
        public readonly Hash128 GlobalId;

        public ObjectIdentifier(int instanceId, Hash128 globalId)
        {
            InstanceId = instanceId;
            GlobalId = globalId;
        }

        public bool Equals(ObjectIdentifier other)
        {
            return InstanceId == other.InstanceId;
        }

        public override bool Equals(object obj)
        {
            return obj is ObjectIdentifier other && Equals(other);
        }

        public override int GetHashCode()
        {
            return InstanceId;
        }

        public override string ToString()
        {
            return $"Instance ID: {InstanceId}, Global ID: {GlobalId}";
        }
    }
}