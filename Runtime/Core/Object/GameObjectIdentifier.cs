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
        
        public static GameObjectIdentifier Null { get; } = new(Identifier.Null, Identifier.Null);
        
        private static readonly uint GameObjectIdFieldTag = WireFormat.MakeTag(1, WireFormat.WireType.LengthDelimited);
        private static readonly uint TransformIdFieldTag = WireFormat.MakeTag(2, WireFormat.WireType.LengthDelimited);
        
        public readonly Identifier GameObjectId;
        public readonly Identifier TransformId;

        public GameObjectIdentifier(Identifier gameObjectId, Identifier transformId)
        {
            GameObjectId = gameObjectId;
            TransformId = transformId;
        }

        public int ComputeSize()
        {
            return BufferWriterExtensions.ComputeTagSize(GameObjectIdFieldTag) +
                   BufferWriterExtensions.ComputeLengthPrefixSize(Guid.Size) +
                   Guid.Size +
                   BufferWriterExtensions.ComputeTagSize(TransformIdFieldTag) +
                   BufferWriterExtensions.ComputeLengthPrefixSize(Guid.Size) +
                   Guid.Size;
        }

        public void WriteTo(ref BufferWriter bufferWriter)
        {
            var gameObjectGuid = GameObjectId.Guid;
            bufferWriter.WriteTag(GameObjectIdFieldTag);
            bufferWriter.WriteLength(Guid.Size);
            gameObjectGuid.WriteTo(ref bufferWriter);
            
            var transformGuid = TransformId.Guid;
            bufferWriter.WriteTag(TransformIdFieldTag);
            bufferWriter.WriteLength(Guid.Size);
            transformGuid.WriteTo(ref bufferWriter);
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