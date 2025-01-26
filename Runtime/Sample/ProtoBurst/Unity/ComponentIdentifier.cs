using System;
using PLUME.Core.Object;
using ProtoBurst;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Burst;
using Unity.Collections;
using Guid = PLUME.Core.Guid;

namespace PLUME.Sample.ProtoBurst.Unity
{
    [BurstCompile]
    public readonly struct ComponentIdentifier : IObjectIdentifier, IProtoBurstMessage, IEquatable<ComponentIdentifier>
    {
        public static readonly FixedString128Bytes TypeUrl = "fr.liris.plume/plume.sample.unity.ComponentIdentifier";

        public static ComponentIdentifier Null { get; } = new(0, Guid.Null, GameObjectIdentifier.Null);

        private static readonly uint ComponentFieldTag =
            WireFormat.MakeTag(Sample.Unity.ComponentIdentifier.GuidFieldNumber, WireFormat.WireType.LengthDelimited);

        private static readonly uint GameObjectFieldTag =
            WireFormat.MakeTag(Sample.Unity.ComponentIdentifier.GameObjectFieldNumber,
                WireFormat.WireType.LengthDelimited);

        public readonly int RuntimeId;
        public readonly Guid Guid;

        public readonly GameObjectIdentifier GameObject;

        public ComponentIdentifier(int runtimeId, Guid guid, GameObjectIdentifier gameObject)
        {
            RuntimeId = runtimeId;
            Guid = guid;
            GameObject = gameObject;
        }

        public int ComputeSize()
        {
            var gameObject = GameObject;

            return BufferWriterExtensions.ComputeTagSize(ComponentFieldTag) +
                   BufferWriterExtensions.ComputeLengthPrefixSize(Guid.Size) +
                   Guid.Size +
                   BufferWriterExtensions.ComputeTagSize(GameObjectFieldTag) +
                   BufferWriterExtensions.ComputeLengthPrefixedMessageSize(ref gameObject);
        }

        public void WriteTo(ref BufferWriter bufferWriter)
        {
            var guid = Guid;
            bufferWriter.WriteTag(ComponentFieldTag);
            bufferWriter.WriteLength(Guid.Size);
            guid.WriteTo(ref bufferWriter);

            var gameObject = GameObject;
            bufferWriter.WriteTag(GameObjectFieldTag);
            bufferWriter.WriteLengthPrefixedMessage(ref gameObject);
        }

        public SampleTypeUrl GetTypeUrl(Allocator allocator)
        {
            return SampleTypeUrl.Alloc(TypeUrl, allocator);
        }

        public bool Equals(ComponentIdentifier other)
        {
            return RuntimeId == other.RuntimeId && Guid.Equals(other.Guid);
        }

        public override int GetHashCode()
        {
            return RuntimeId;
        }
    }
}