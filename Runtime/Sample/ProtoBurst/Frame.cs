using System;
using ProtoBurst;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Sample.ProtoBurst
{
    [BurstCompile]
    public struct Frame : IProtoBurstMessage, IDisposable
    {
        public static readonly FixedString128Bytes TypeUrl = "fr.liris.plume/plume.sample.unity.Frame";
        
        public static readonly uint FrameNumberFieldTag = WireFormat.MakeTag(1, WireFormat.WireType.VarInt);
        public static readonly uint DataFieldTag = WireFormat.MakeTag(2, WireFormat.WireType.LengthDelimited);
        
        private readonly int _frameNumber;
        private NativeList<byte> _frameDataRawBytes;

        private Frame(int frameNumber, NativeList<byte> frameDataRawBytes)
        {
            _frameNumber = frameNumber;
            _frameDataRawBytes = frameDataRawBytes;
        }

        public static Frame Pack(int frameNumber, ref NativeList<byte> frameDataRawBytes, Allocator allocator)
        {
            var frameDataRawBytesCopy = new NativeList<byte>(frameDataRawBytes.Length, allocator);
            frameDataRawBytesCopy.AddRange(frameDataRawBytes.AsArray());
            return new Frame(frameNumber, frameDataRawBytesCopy);
        }
        
        public void WriteTo(ref BufferWriter bufferWriter)
        {
            bufferWriter.WriteTag(FrameNumberFieldTag);
            bufferWriter.WriteInt32(_frameNumber);
            bufferWriter.WriteBytes(ref _frameDataRawBytes);
        }

        public int ComputeSize()
        {
            var size = BufferWriterExtensions.ComputeTagSize(FrameNumberFieldTag) +
                       BufferWriterExtensions.ComputeInt32Size(_frameNumber) +
                       _frameDataRawBytes.Length;

            return size;
        }
        
        public static void WriteTo(int frameNumber, ref NativeArray<byte> frameDataRawBytes, ref BufferWriter bufferWriter)
        {
            bufferWriter.WriteTag(FrameNumberFieldTag);
            bufferWriter.WriteInt32(frameNumber);
            bufferWriter.WriteBytes(ref frameDataRawBytes);
        }

        public SampleTypeUrl GetTypeUrl(Allocator allocator)
        {
            return SampleTypeUrl.Alloc(TypeUrl, allocator);
        }

        public void Dispose()
        {
            _frameDataRawBytes.Dispose();
        }
    }
}