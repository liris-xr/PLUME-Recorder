using System;
using PLUME.Core.Object;
using ProtoBurst;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Sample.ProtoBurst
{
    [BurstCompile]
    public readonly struct ComponentIdentifier : IProtoBurstMessage, IEquatable<ComponentIdentifier>
    {
        public static readonly FixedString128Bytes TypeUrl =
            "fr.liris.plume/plume.sample.unity.ComponentIdentifier";

        public static ComponentIdentifier Null { get; } = new(ObjectIdentifier.Null, ObjectIdentifier.Null);

        private static readonly uint ComponentGuidFieldTag = WireFormat.MakeTag(1, WireFormat.WireType.LengthDelimited);
        private static readonly uint ParentGuidFieldTag = WireFormat.MakeTag(2, WireFormat.WireType.LengthDelimited);

        private readonly ObjectIdentifier _componentIdentifier;
        private readonly ObjectIdentifier _parentIdentifier;

        public ComponentIdentifier(ObjectIdentifier componentIdentifier, ObjectIdentifier parentIdentifier)
        {
            _componentIdentifier = componentIdentifier;
            _parentIdentifier = parentIdentifier;
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
            var componentGuid = _componentIdentifier.GlobalId;
            var parentGuid = _parentIdentifier.GlobalId;

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
            return _componentIdentifier.Equals(other._componentIdentifier) &&
                   _parentIdentifier.Equals(other._parentIdentifier);
        }

        public override bool Equals(object obj)
        {
            return obj is ComponentIdentifier other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_componentIdentifier, _parentIdentifier);
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