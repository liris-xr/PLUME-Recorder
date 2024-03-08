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

        public static AssetIdentifier Null { get; } = new(Identifier.Null, "");

        private static readonly uint AssetIdFieldTag = WireFormat.MakeTag(1, WireFormat.WireType.LengthDelimited);
        private static readonly uint AssetPathFieldTag = WireFormat.MakeTag(2, WireFormat.WireType.LengthDelimited);

        public readonly Identifier AssetId;
        public readonly FixedString512Bytes AssetPath;

        public AssetIdentifier(Identifier assetId, FixedString512Bytes assetPath)
        {
            AssetId = assetId;
            AssetPath = assetPath;
        }

        public int ComputeSize()
        {
            var path = AssetPath;
            
            return BufferWriterExtensions.ComputeTagSize(AssetIdFieldTag) +
                   BufferWriterExtensions.ComputeLengthPrefixSize(Guid.Size) +
                   Guid.Size +
                   BufferWriterExtensions.ComputeTagSize(AssetPathFieldTag) +
                   BufferWriterExtensions.ComputeLengthPrefixedFixedString(ref path);
        }

        public void WriteTo(ref BufferWriter bufferWriter)
        {
            var assetGuid = AssetId.Guid;
            bufferWriter.WriteTag(AssetIdFieldTag);
            bufferWriter.WriteLength(Guid.Size);
            assetGuid.WriteTo(ref bufferWriter);

            var path = AssetPath;
            bufferWriter.WriteTag(AssetPathFieldTag);
            bufferWriter.WriteLengthPrefixedFixedString(ref path);
        }

        public SampleTypeUrl GetTypeUrl(Allocator allocator)
        {
            return SampleTypeUrl.Alloc(TypeUrl, allocator);
        }

        public bool Equals(AssetIdentifier other)
        {
            return AssetId.Equals(other.AssetId);
        }

        public bool Equals(IObjectIdentifier other)
        {
            return other is AssetIdentifier assetIdentifier && Equals(assetIdentifier);
        }

        [BurstDiscard]
        public override bool Equals(object obj)
        {
            return obj is AssetIdentifier other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            return AssetId.GetHashCode();
        }
    }
}