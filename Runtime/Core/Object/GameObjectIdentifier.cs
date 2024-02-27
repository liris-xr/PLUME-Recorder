using System;
using ProtoBurst;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Core.Object
{
    [BurstCompile]
    public readonly struct GameObjectIdentifier : IObjectIdentifier, IProtoBurstMessage, IEquatable<GameObjectIdentifier>
    {
        public static readonly FixedString128Bytes TypeUrl = "fr.liris.plume/plume.sample.unity.GameObjectIdentifier";
        
        public static GameObjectIdentifier Null { get; } = new(ObjectIdentifier.Null, ObjectIdentifier.Null);
        
        private static readonly uint GameObjectIdFieldTag = WireFormat.MakeTag(1, WireFormat.WireType.LengthDelimited);
        private static readonly uint TransformIdFieldTag = WireFormat.MakeTag(2, WireFormat.WireType.LengthDelimited);
        
        public readonly ObjectIdentifier GameObjectId;
        public readonly ObjectIdentifier TransformId;

        public GameObjectIdentifier(ObjectIdentifier gameObjectId, ObjectIdentifier transformId)
        {
            GameObjectId = gameObjectId;
            TransformId = transformId;
        }

        public int ComputeSize()
        {
            var gameObjectId = GameObjectId;
            var transformId = TransformId;
            
            return BufferWriterExtensions.ComputeTagSize(GameObjectIdFieldTag) +
                   BufferWriterExtensions.ComputeLengthPrefixedMessageSize(ref gameObjectId) +
                   BufferWriterExtensions.ComputeTagSize(TransformIdFieldTag) +
                   BufferWriterExtensions.ComputeLengthPrefixedMessageSize(ref transformId);
        }

        public void WriteTo(ref BufferWriter bufferWriter)
        {
            var gameObjectId = GameObjectId;
            var transformId = TransformId;
            
            bufferWriter.WriteTag(GameObjectIdFieldTag);
            bufferWriter.WriteLengthPrefixedMessage(ref gameObjectId);
            bufferWriter.WriteTag(TransformIdFieldTag);
            bufferWriter.WriteLengthPrefixedMessage(ref transformId);
        }

        public SampleTypeUrl GetTypeUrl(Allocator allocator)
        {
            return SampleTypeUrl.Alloc(TypeUrl, allocator);
        }

        public bool Equals(GameObjectIdentifier other)
        {
            return GameObjectId.Equals(other.GameObjectId) &&
                   TransformId.Equals(other.TransformId);
        }
        
        public override bool Equals(object obj)
        {
            return obj is GameObjectIdentifier other && Equals(other);
        }

        public override int GetHashCode()
        {
            var hash = 23;
            hash = hash * 37 + GameObjectId.GetHashCode();
            hash = hash * 37 + TransformId.GetHashCode();
            return hash;
        }

        public static bool operator ==(GameObjectIdentifier lhs, GameObjectIdentifier rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(GameObjectIdentifier lhs, GameObjectIdentifier rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}