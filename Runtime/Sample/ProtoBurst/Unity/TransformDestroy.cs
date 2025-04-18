using ProtoBurst;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Sample.ProtoBurst.Unity
{
    [BurstCompile]
    public struct TransformDestroy : IProtoBurstMessage
    {
        public static readonly FixedString128Bytes TypeUrl = "fr.liris.plume/plume.sample.unity.TransformDestroy";

        private static readonly uint IdentifierFieldTag = WireFormat.MakeTag(Sample.Unity.TransformDestroy.ComponentFieldNumber, WireFormat.WireType.LengthDelimited);

        private ComponentIdentifier _identifier;

        public TransformDestroy(ComponentIdentifier identifier)
        {
            _identifier = identifier;
        }

        public int ComputeSize()
        {
            return BufferWriterExtensions.ComputeTagSize(IdentifierFieldTag) +
                   BufferWriterExtensions.ComputeLengthPrefixedMessageSize(ref _identifier);
        }

        public void WriteTo(ref BufferWriter bufferWriter)
        {
            bufferWriter.WriteTag(IdentifierFieldTag);
            bufferWriter.WriteLengthPrefixedMessage(ref _identifier);
        }

        public SampleTypeUrl GetTypeUrl(Allocator allocator)
        {
            return SampleTypeUrl.Alloc(TypeUrl, allocator);
        }
    }
}