using System;
using System.Runtime.CompilerServices;
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
        public static readonly SampleTypeUrl FrameSampleTypeUrl =
            SampleTypeUrlRegistry.GetOrCreate("fr.liris.plume", Frame.Descriptor);

        public SampleTypeUrl TypeUrl => FrameSampleTypeUrl;

        public readonly int FrameNumber;
        public NativeArray<byte> Data;

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

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteToNoResize(ref NativeList<byte> data)
        {
            WritingPrimitives.WriteTagNoResize(Frame.FrameNumberFieldNumber, WireFormat.WireType.VarInt, ref data);
            WritingPrimitives.WriteInt32NoResize(FrameNumber, ref data);

            WritingPrimitives.WriteTagNoResize(Frame.DataFieldNumber, WireFormat.WireType.LengthDelimited, ref data);
            WritingPrimitives.WriteLengthPrefixedBytesNoResize(ref Data, ref data);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteTo(ref NativeList<byte> data)
        {
            WritingPrimitives.WriteTag(Frame.FrameNumberFieldNumber, WireFormat.WireType.VarInt, ref data);
            WritingPrimitives.WriteInt32(FrameNumber, ref data);

            WritingPrimitives.WriteTag(Frame.DataFieldNumber, WireFormat.WireType.LengthDelimited, ref data);
            WritingPrimitives.WriteLengthPrefixedBytes(ref Data, ref data);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ComputeMaxSize()
        {
            var size = WritingPrimitives.TagSize + WritingPrimitives.Int32MaxSize;
            size += WritingPrimitives.TagSize + WritingPrimitives.LengthPrefixMaxSize + Data.Length;
            return size;
        }

        public void Dispose()
        {
            Data.Dispose();
        }
    }
}