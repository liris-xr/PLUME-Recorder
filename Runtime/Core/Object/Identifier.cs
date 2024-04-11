using System;
using Unity.Burst;

namespace PLUME.Core.Object
{
    [BurstCompile]
    public readonly struct Identifier : IEquatable<Identifier>
    {
        public static Identifier Null { get; } = new(0, Guid.Null);

        public readonly int InstanceId;
        public readonly Guid Guid;

        public Identifier(int instanceId, Guid guid)
        {
            InstanceId = instanceId;
            Guid = guid;
        }

        public bool Equals(Identifier other)
        {
            return InstanceId == other.InstanceId && Guid.Equals(other.Guid);
        }

        public override int GetHashCode()
        {
            return InstanceId;
        }
    }
}