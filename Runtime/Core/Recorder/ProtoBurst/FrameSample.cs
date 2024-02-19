using System;
using PLUME.Sample.Unity;
using ProtoBurst;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Core.Recorder.ProtoBurst
{
    [BurstCompile]
    public struct FrameSample : IProtoBurstMessage, IDisposable
    {
        public static readonly FixedString128Bytes TypeUrl = new("fr.liris.plume/" + Frame.Descriptor.FullName);

        public readonly int FrameNumber;
        public NativeArray<byte> Data;

        private static readonly uint FrameNumberFieldTag = WireFormat.MakeTag(1, WireFormat.WireType.VarInt);
        private static readonly uint DataFieldTag = WireFormat.MakeTag(2, WireFormat.WireType.LengthDelimited);

        public FrameSample(int frameNumber, NativeArray<byte> data)
        {
            FrameNumber = frameNumber;
            Data = data;
        }

        public static FrameSample Pack(int frameNumber, ReadOnlySpan<byte> data, Allocator allocator)
        {
            var dataArr = new NativeArray<byte>(data.Length, allocator);
            data.CopyTo(dataArr);
            return new FrameSample(frameNumber, dataArr);
        }

        public static FrameSample Pack(int frameNumber, NativeArray<byte> data)
        {
            return new FrameSample(frameNumber, data);
        }

        public void WriteTo(ref BufferWriter bufferWriter)
        {
            bufferWriter.WriteTag(FrameNumberFieldTag);
            bufferWriter.WriteSInt32(FrameNumber);
            bufferWriter.WriteTag(DataFieldTag);
            bufferWriter.WriteLengthPrefixedBytes(ref Data);
        }
        
        public static void WriteTo(int frameNumber, NativeArray<byte> data, ref BufferWriter bufferWriter)
        {
            bufferWriter.WriteTag(FrameNumberFieldTag);
            bufferWriter.WriteSInt32(frameNumber);
            bufferWriter.WriteTag(DataFieldTag);
            bufferWriter.WriteLengthPrefixedBytes(ref data);
        }

        public SampleTypeUrl GetTypeUrl(Allocator allocator)
        {
            return SampleTypeUrl.Alloc(TypeUrl, allocator);
        }

        public int ComputeSize()
        {
            return BufferExtensions.TagSize * 2 +
                   BufferExtensions.ComputeVarIntSize(FrameNumber) +
                   BufferExtensions.ComputeLengthPrefixedBytesSize(ref Data);
        }

        public void Dispose()
        {
            Data.Dispose();
        }
    }
}