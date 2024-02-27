using System;
using ProtoBurst;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Core.Object
{
    [BurstCompile]
    public readonly struct AssetIdentifier : IObjectIdentifier, IProtoBurstMessage, IEquatable<AssetIdentifier>
    {
        public static readonly FixedString128Bytes TypeUrl = "fr.liris.plume/plume.sample.unity.AssetIdentifier";

        public static AssetIdentifier Null { get; } = new(ObjectIdentifier.Null, "");

        private static readonly uint AssetGuidFieldTag = WireFormat.MakeTag(1, WireFormat.WireType.LengthDelimited);
        private static readonly uint AssetPathFieldTag = WireFormat.MakeTag(2, WireFormat.WireType.LengthDelimited);

        public readonly ObjectIdentifier Id;
        public readonly FixedString512Bytes Path;

        public AssetIdentifier(ObjectIdentifier id, FixedString512Bytes path)
        {
            Id = id;
            Path = path;
        }

        public int ComputeSize()
        {
            var path = Path;
            
            return BufferWriterExtensions.ComputeTagSize(AssetGuidFieldTag) +
                   BufferWriterExtensions.ComputeLengthPrefixSize(Guid.Size) +
                   Guid.Size +
                   BufferWriterExtensions.ComputeTagSize(AssetPathFieldTag) +
                   BufferWriterExtensions.ComputeLengthPrefixedFixedString(ref path);
        }

        public void WriteTo(ref BufferWriter bufferWriter)
        {
            var assetGuid = Id.Guid;
            var path = Path;

            bufferWriter.WriteTag(AssetGuidFieldTag);
            bufferWriter.WriteLength(Guid.Size);
            assetGuid.WriteTo(ref bufferWriter);

            bufferWriter.WriteTag(AssetPathFieldTag);
            bufferWriter.WriteLengthPrefixedFixedString(ref path);
        }

        public SampleTypeUrl GetTypeUrl(Allocator allocator)
        {
            return SampleTypeUrl.Alloc(TypeUrl, allocator);
        }

        public bool Equals(AssetIdentifier other)
        {
            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            return obj is AssetIdentifier other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(AssetIdentifier lhs, AssetIdentifier rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(AssetIdentifier lhs, AssetIdentifier rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}