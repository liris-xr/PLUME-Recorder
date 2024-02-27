using System;
using ProtoBurst;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Core.Object
{
    [BurstCompile]
    public readonly struct ObjectIdentifier : IObjectIdentifier, IProtoBurstMessage, IEquatable<ObjectIdentifier>
    {
        public static readonly FixedString128Bytes TypeUrl = "fr.liris.plume/plume.sample.unity.ObjectIdentifier";

        public static ObjectIdentifier Null { get; } = new(0, Guid.Null);

        private static readonly uint IdFieldTag = WireFormat.MakeTag(1, WireFormat.WireType.LengthDelimited);

        public readonly int InstanceId;
        
        public readonly Guid Guid;
        
        public ObjectIdentifier(int instanceId, Guid guid)
        {
            InstanceId = instanceId;
            Guid = guid;
        }

        public int ComputeSize()
        {
            return BufferWriterExtensions.ComputeTagSize(IdFieldTag) +
                   BufferWriterExtensions.ComputeLengthPrefixSize(Guid.Size) +
                   Guid.Size;
        }

        public void WriteTo(ref BufferWriter bufferWriter)
        {
            var guid = Guid;

            bufferWriter.WriteTag(IdFieldTag);
            bufferWriter.WriteLength(Guid.Size);
            guid.WriteTo(ref bufferWriter);
        }

        public SampleTypeUrl GetTypeUrl(Allocator allocator)
        {
            return SampleTypeUrl.Alloc(TypeUrl, allocator);
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

        public static bool operator ==(ObjectIdentifier lhs, ObjectIdentifier rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(ObjectIdentifier lhs, ObjectIdentifier rhs)
        {
            return !(lhs == rhs);
        }
        
        public override string ToString()
        {
            return $"Instance ID: {InstanceId}, Global ID: {Guid}";
        }
    }
}