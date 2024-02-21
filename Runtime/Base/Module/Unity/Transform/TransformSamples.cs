using System.Runtime.InteropServices;
using PLUME.Sample.ProtoBurst;
using ProtoBurst;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Base.Module.Unity.Transform
{
    [BurstCompile]
    [StructLayout(LayoutKind.Sequential)]
    public struct TransformUpdateLocalPositionSample : IProtoBurstMessage
    {
        public static readonly FixedString128Bytes TypeUrl =
            "fr.liris.plume/plume.sample.unity.TransformUpdateLocalPosition";

        private static readonly uint IdentifierFieldTag = WireFormat.MakeTag(1, WireFormat.WireType.LengthDelimited);
        private static readonly uint LocalPositionFieldTag = WireFormat.MakeTag(2, WireFormat.WireType.LengthDelimited);

        // TODO: add identifier

        public Vector3Sample LocalPosition;

        public TransformUpdateLocalPositionSample(Vector3Sample localPosition)
        {
            LocalPosition = localPosition;
        }
        
        public void WriteTo(ref BufferWriter bufferWriter)
        {
            bufferWriter.WriteTag(LocalPositionFieldTag);
            bufferWriter.WriteLengthPrefixedMessage(ref LocalPosition);
        }

        public int ComputeSize()
        {
            return BufferWriterExtensions.ComputeTagSize(LocalPositionFieldTag) + BufferWriterExtensions.ComputeLengthPrefixedMessageSize(ref LocalPosition);
        }

        public SampleTypeUrl GetTypeUrl(Allocator allocator)
        {
            return SampleTypeUrl.Alloc(TypeUrl, allocator);
        }
    }
}