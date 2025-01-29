using ProtoBurst;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Sample.ProtoBurst.Unity
{
    [BurstCompile]
    public struct TransformCreate : IProtoBurstMessage
    {
        public static readonly FixedString128Bytes TypeUrl = "fr.liris.plume/plume.sample.unity.TransformCreate";

        private static readonly uint IdentifierFieldTag = WireFormat.MakeTag(Sample.Unity.TransformCreate.ComponentFieldNumber, WireFormat.WireType.LengthDelimited);

        private ComponentIdentifier _identifier;

        public TransformCreate(ComponentIdentifier identifier)
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