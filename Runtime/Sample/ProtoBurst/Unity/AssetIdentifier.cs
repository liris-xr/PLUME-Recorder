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
    public readonly struct AssetIdentifier : IObjectIdentifier, IProtoBurstMessage, IEquatable<AssetIdentifier>
    {
        public static readonly FixedString128Bytes TypeUrl = "fr.liris.plume/plume.sample.unity.AssetIdentifier";

        public static AssetIdentifier Null { get; } = new(0, Guid.Null, "");

        private static readonly uint GuidFieldTag = WireFormat.MakeTag(Sample.Unity.AssetIdentifier.GuidFieldNumber,
            WireFormat.WireType.LengthDelimited);

        private static readonly uint AssetPathFieldTag =
            WireFormat.MakeTag(Sample.Unity.AssetIdentifier.AssetBundlePathFieldNumber, WireFormat.WireType.LengthDelimited);

        public readonly int RuntimeId;
        public readonly Guid Guid;
        public readonly FixedString512Bytes AssetPath;

        public AssetIdentifier(int runtimeId, Guid guid, FixedString512Bytes assetPath)
        {
            RuntimeId = runtimeId;
            Guid = guid;
            AssetPath = assetPath;
        }

        public int ComputeSize()
        {
            var path = AssetPath;

            return BufferWriterExtensions.ComputeTagSize(GuidFieldTag) +
                   BufferWriterExtensions.ComputeLengthPrefixSize(Guid.Size) +
                   Guid.Size +
                   BufferWriterExtensions.ComputeTagSize(AssetPathFieldTag) +
                   BufferWriterExtensions.ComputeLengthPrefixedFixedString(ref path);
        }

        public void WriteTo(ref BufferWriter bufferWriter)
        {
            var assetGuid = Guid;
            bufferWriter.WriteTag(GuidFieldTag);
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
            return RuntimeId.Equals(other.RuntimeId);
        }

        public override int GetHashCode()
        {
            return RuntimeId.GetHashCode();
        }
    }
}