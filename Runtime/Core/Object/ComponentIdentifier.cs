using System;
using ProtoBurst;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Core.Object
{
    [BurstCompile]
    public readonly struct ComponentIdentifier : IObjectIdentifier, IProtoBurstMessage, IEquatable<ComponentIdentifier>
    {
        public static readonly FixedString128Bytes TypeUrl = "fr.liris.plume/plume.sample.unity.ComponentIdentifier";

        public static ComponentIdentifier Null { get; } =
            new(ObjectIdentifier.Null, ObjectIdentifier.Null);

        private static readonly uint ComponentGuidFieldTag = WireFormat.MakeTag(1, WireFormat.WireType.LengthDelimited);
        private static readonly uint ParentGuidFieldTag = WireFormat.MakeTag(2, WireFormat.WireType.LengthDelimited);

        public readonly ObjectIdentifier ComponentId;
        public readonly ObjectIdentifier ParentId;

        public ComponentIdentifier(ObjectIdentifier componentId, ObjectIdentifier parentId)
        {
            ComponentId = componentId;
            ParentId = parentId;
        }

        public int ComputeSize()
        {
            return BufferWriterExtensions.ComputeTagSize(ComponentGuidFieldTag) +
                   BufferWriterExtensions.ComputeLengthPrefixSize(Guid.Size) +
                   Guid.Size +
                   BufferWriterExtensions.ComputeTagSize(ParentGuidFieldTag) +
                   BufferWriterExtensions.ComputeLengthPrefixSize(Guid.Size) +
                   Guid.Size;
        }

        public void WriteTo(ref BufferWriter bufferWriter)
        {
            var componentGuid = ComponentId.Guid;
            var parentGuid = ParentId.Guid;

            bufferWriter.WriteTag(ComponentGuidFieldTag);
            bufferWriter.WriteLength(Guid.Size);
            componentGuid.WriteTo(ref bufferWriter);

            bufferWriter.WriteTag(ParentGuidFieldTag);
            bufferWriter.WriteLength(Guid.Size);
            parentGuid.WriteTo(ref bufferWriter);
        }

        public SampleTypeUrl GetTypeUrl(Allocator allocator)
        {
            return SampleTypeUrl.Alloc(TypeUrl, allocator);
        }

        public bool Equals(ComponentIdentifier other)
        {
            return ComponentId.Equals(other.ComponentId) &&
                   ParentId.Equals(other.ParentId);
        }

        public override bool Equals(object obj)
        {
            return obj is ComponentIdentifier other && Equals(other);
        }

        public override int GetHashCode()
        {
            var hash = 23;
            hash = hash * 37 + ComponentId.GetHashCode();
            hash = hash * 37 + ParentId.GetHashCode();
            return hash;
        }

        public static bool operator ==(ComponentIdentifier lhs, ComponentIdentifier rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(ComponentIdentifier lhs, ComponentIdentifier rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}