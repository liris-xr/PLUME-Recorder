using System;
using PLUME.Core.Object;
using ProtoBurst;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Sample.ProtoBurst
{
    [BurstCompile]
    public readonly struct TransformGameObjectIdentifier : IProtoBurstMessage, IEquatable<TransformGameObjectIdentifier>
    {
        public static readonly FixedString128Bytes TypeUrl =
            "fr.liris.plume/plume.sample.unity.TransformGameObjectIdentifier";

        public static TransformGameObjectIdentifier Null { get; } = new(ObjectIdentifier.Null, ObjectIdentifier.Null);

        private static readonly uint TransformGuidFieldTag = WireFormat.MakeTag(1, WireFormat.WireType.LengthDelimited);

        private static readonly uint GameObjectGuidFieldTag =
            WireFormat.MakeTag(2, WireFormat.WireType.LengthDelimited);

        private readonly ObjectIdentifier _transformIdentifier;
        private readonly ObjectIdentifier _gameObjectIdentifier;

        public TransformGameObjectIdentifier(ObjectIdentifier transformIdentifier,
            ObjectIdentifier gameObjectIdentifier)
        {
            _transformIdentifier = transformIdentifier;
            _gameObjectIdentifier = gameObjectIdentifier;
        }

        public int ComputeSize()
        {
            return BufferWriterExtensions.ComputeTagSize(TransformGuidFieldTag) +
                   BufferWriterExtensions.ComputeLengthPrefixSize(Guid.Size) +
                   Guid.Size +
                   BufferWriterExtensions.ComputeTagSize(GameObjectGuidFieldTag) +
                   BufferWriterExtensions.ComputeLengthPrefixSize(Guid.Size) +
                   Guid.Size;
        }

        public void WriteTo(ref BufferWriter bufferWriter)
        {
            var transformGuid = _transformIdentifier.GlobalId;
            var goGuid = _gameObjectIdentifier.GlobalId;

            bufferWriter.WriteTag(TransformGuidFieldTag);
            bufferWriter.WriteLength(Guid.Size);
            transformGuid.WriteTo(ref bufferWriter);

            bufferWriter.WriteTag(GameObjectGuidFieldTag);
            bufferWriter.WriteLength(Guid.Size);
            goGuid.WriteTo(ref bufferWriter);
        }

        public SampleTypeUrl GetTypeUrl(Allocator allocator)
        {
            return SampleTypeUrl.Alloc(TypeUrl, allocator);
        }

        public bool Equals(TransformGameObjectIdentifier other)
        {
            return _transformIdentifier.Equals(other._transformIdentifier) &&
                   _gameObjectIdentifier.Equals(other._gameObjectIdentifier);
        }

        public override bool Equals(object obj)
        {
            return obj is TransformGameObjectIdentifier other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_transformIdentifier, _gameObjectIdentifier);
        }

        public static bool operator ==(TransformGameObjectIdentifier lhs, TransformGameObjectIdentifier rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(TransformGameObjectIdentifier lhs, TransformGameObjectIdentifier rhs)
        {
            return !(lhs == rhs);
        }
    }
}