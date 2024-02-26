using System;
using UnityEngine;
using Guid = PLUME.Sample.ProtoBurst.Guid;

namespace PLUME.Core.Object
{
    public readonly struct ObjectIdentifier : IEquatable<ObjectIdentifier>
    {
        public static ObjectIdentifier Null { get; } = new(0, Guid.Null);

        public readonly int InstanceId;
        public readonly Guid GlobalId;

        public ObjectIdentifier(int instanceId, Guid globalId)
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