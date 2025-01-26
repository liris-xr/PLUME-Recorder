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
    public readonly struct SceneIdentifier : IObjectIdentifier, IProtoBurstMessage, IEquatable<SceneIdentifier>
    {
        public static readonly FixedString128Bytes TypeUrl = "fr.liris.plume/plume.sample.unity.SceneIdentifier";

        public static SceneIdentifier Null { get; } = new(Guid.Null, "", "");

        private static readonly uint GuidFieldTag =
            WireFormat.MakeTag(Sample.Unity.SceneIdentifier.GuidFieldNumber, WireFormat.WireType.LengthDelimited);

        private static readonly uint NameFieldTag =
            WireFormat.MakeTag(Sample.Unity.SceneIdentifier.GuidFieldNumber, WireFormat.WireType.LengthDelimited);

        private static readonly uint PathFieldTag =
            WireFormat.MakeTag(Sample.Unity.SceneIdentifier.AssetBundlePathFieldNumber,
                WireFormat.WireType.LengthDelimited);

        public readonly Guid Guid;
        public readonly FixedString512Bytes Name;
        public readonly FixedString512Bytes Path;

        public SceneIdentifier(Guid guid, FixedString512Bytes name, FixedString512Bytes path)
        {
            Guid = guid;
            Name = name;
            Path = path;
        }

        public int ComputeSize()
        {
            var path = Path;
            var name = Name;

            return BufferWriterExtensions.ComputeTagSize(GuidFieldTag) +
                   BufferWriterExtensions.ComputeLengthPrefixSize(Guid.Size) +
                   Guid.Size +
                   BufferWriterExtensions.ComputeTagSize(NameFieldTag) +
                   BufferWriterExtensions.ComputeLengthPrefixedFixedString(ref name) +
                   BufferWriterExtensions.ComputeTagSize(PathFieldTag) +
                   BufferWriterExtensions.ComputeLengthPrefixedFixedString(ref path);
        }

        public void WriteTo(ref BufferWriter bufferWriter)
        {
            var guid = Guid;
            bufferWriter.WriteTag(GuidFieldTag);
            bufferWriter.WriteLength(Guid.Size);
            guid.WriteTo(ref bufferWriter);

            var name = Name;
            bufferWriter.WriteTag(NameFieldTag);
            bufferWriter.WriteLengthPrefixedFixedString(ref name);

            var path = Path;
            bufferWriter.WriteTag(PathFieldTag);
            bufferWriter.WriteLengthPrefixedFixedString(ref path);
        }

        public SampleTypeUrl GetTypeUrl(Allocator allocator)
        {
            return SampleTypeUrl.Alloc(TypeUrl, allocator);
        }

        public bool Equals(SceneIdentifier other)
        {
            return Guid.Equals(other.Guid);
        }

        public override int GetHashCode()
        {
            return Guid.GetHashCode();
        }
    }
}