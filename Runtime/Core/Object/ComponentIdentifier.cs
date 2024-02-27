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

        public static ComponentIdentifier Null { get; } = new(Identifier.Null, GameObjectIdentifier.Null);

        private static readonly uint ComponentIdFieldTag = WireFormat.MakeTag(1, WireFormat.WireType.LengthDelimited);
        private static readonly uint GameObjectIdFieldTag = WireFormat.MakeTag(2, WireFormat.WireType.LengthDelimited);

        public readonly Identifier ComponentId;
        public readonly GameObjectIdentifier GameObjectId;

        public ComponentIdentifier(Identifier componentId, GameObjectIdentifier gameObjectId)
        {
            ComponentId = componentId;
            GameObjectId = gameObjectId;
        }

        public int ComputeSize()
        {
            var gameObjectId = GameObjectId;
            
            return BufferWriterExtensions.ComputeTagSize(ComponentIdFieldTag) +
                   BufferWriterExtensions.ComputeLengthPrefixSize(Guid.Size) +
                   Guid.Size +
                   BufferWriterExtensions.ComputeTagSize(GameObjectIdFieldTag) +
                   BufferWriterExtensions.ComputeLengthPrefixedMessageSize(ref gameObjectId);
        }

        public void WriteTo(ref BufferWriter bufferWriter)
        {
            var componentGuid = ComponentId.Guid;
            bufferWriter.WriteTag(ComponentIdFieldTag);
            bufferWriter.WriteLength(Guid.Size);
            componentGuid.WriteTo(ref bufferWriter);
            
            var gameObjectId = GameObjectId;
            bufferWriter.WriteTag(GameObjectIdFieldTag);
            bufferWriter.WriteLengthPrefixedMessage(ref gameObjectId);
        }

        public SampleTypeUrl GetTypeUrl(Allocator allocator)
        {
            return SampleTypeUrl.Alloc(TypeUrl, allocator);
        }

        public bool Equals(ComponentIdentifier other)
        {
            return ComponentId.Equals(other.ComponentId) &&
                   GameObjectId.Equals(other.GameObjectId);
        }

        public override bool Equals(object obj)
        {
            return obj is ComponentIdentifier other && Equals(other);
        }

        public override int GetHashCode()
        {
            var hash = 23;
            hash = hash * 37 + ComponentId.GetHashCode();
            hash = hash * 37 + GameObjectId.GetHashCode();
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