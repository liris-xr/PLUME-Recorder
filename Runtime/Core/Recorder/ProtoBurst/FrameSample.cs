using System;
using ProtoBurst;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Core.Recorder.ProtoBurst
{
    [BurstCompile]
    public struct FrameSample : IProtoBurstMessage, IDisposable
    {
        public static readonly FixedString128Bytes TypeUrl = "fr.liris.plume/plume.sample.unity.Frame";

        public readonly int FrameNumber;
        public NativeList<byte> FrameDataRawBytes;

        public static readonly uint FrameNumberFieldTag = WireFormat.MakeTag(1, WireFormat.WireType.VarInt);
        public static readonly uint DataFieldTag = WireFormat.MakeTag(2, WireFormat.WireType.LengthDelimited);

        public FrameSample(int frameNumber, NativeList<byte> frameDataRawBytes)
        {
            FrameNumber = frameNumber;
            FrameDataRawBytes = frameDataRawBytes;
        }

        public void WriteTo(ref BufferWriter bufferWriter)
        {
            bufferWriter.WriteTag(FrameNumberFieldTag);
            bufferWriter.WriteInt32(FrameNumber);
            bufferWriter.WriteBytes(ref FrameDataRawBytes);
        }

        public int ComputeSize()
        {
            var size = BufferExtensions.ComputeTagSize(FrameNumberFieldTag) +
                       BufferExtensions.ComputeInt32Size(FrameNumber) +
                       FrameDataRawBytes.Length;

            return size;
        }

        public SampleTypeUrl GetTypeUrl(Allocator allocator)
        {
            return SampleTypeUrl.Alloc(TypeUrl, allocator);
        }

        public void Dispose()
        {
            FrameDataRawBytes.Dispose();
        }
    }
}