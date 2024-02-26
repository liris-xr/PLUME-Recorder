using System;
using PLUME.Core.Object;
using ProtoBurst;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Sample.ProtoBurst
{
    [BurstCompile]
    public struct AssetIdentifier : IProtoBurstMessage, IEquatable<AssetIdentifier>
    {
        public static readonly FixedString128Bytes TypeUrl = "fr.liris.plume/plume.sample.unity.AssetIdentifier";

        public static AssetIdentifier Null { get; } = new(ObjectIdentifier.Null, "");

        private static readonly uint AssetGuidFieldTag = WireFormat.MakeTag(1, WireFormat.WireType.LengthDelimited);
        private static readonly uint AssetPathFieldTag = WireFormat.MakeTag(2, WireFormat.WireType.LengthDelimited);

        private readonly ObjectIdentifier _assetIdentifier;
        private FixedString512Bytes _assetPath;

        public AssetIdentifier(ObjectIdentifier assetIdentifier, FixedString512Bytes assetPath)
        {
            _assetIdentifier = assetIdentifier;
            _assetPath = assetPath;
        }

        public int ComputeSize()
        {
            return BufferWriterExtensions.ComputeTagSize(AssetGuidFieldTag) +
                   BufferWriterExtensions.ComputeLengthPrefixSize(Guid.Size) +
                   Guid.Size +
                   BufferWriterExtensions.ComputeTagSize(AssetPathFieldTag) +
                   BufferWriterExtensions.ComputeLengthPrefixedFixedString(ref _assetPath);
        }

        public void WriteTo(ref BufferWriter bufferWriter)
        {
            var assetGuid = _assetIdentifier.GlobalId;

            bufferWriter.WriteTag(AssetGuidFieldTag);
            bufferWriter.WriteLength(Guid.Size);
            assetGuid.WriteTo(ref bufferWriter);

            bufferWriter.WriteTag(AssetPathFieldTag);
            bufferWriter.WriteLengthPrefixedFixedString(ref _assetPath);
        }

        public SampleTypeUrl GetTypeUrl(Allocator allocator)
        {
            return SampleTypeUrl.Alloc(TypeUrl, allocator);
        }

        public bool Equals(AssetIdentifier other)
        {
            return _assetIdentifier.Equals(other._assetIdentifier);
        }

        public override bool Equals(object obj)
        {
            return obj is AssetIdentifier other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _assetIdentifier.GetHashCode();
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