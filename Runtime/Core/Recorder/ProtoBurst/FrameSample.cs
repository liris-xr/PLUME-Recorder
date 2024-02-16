using System;
using System.Runtime.CompilerServices;
using PLUME.Sample.Unity;
using ProtoBurst;
using ProtoBurst.Message;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Core.Recorder.ProtoBurst
{
    [BurstCompile]
    public struct FrameSample : IProtoBurstMessage, IDisposable
    {
        public static FixedString128Bytes FrameSampleTypeUrl => "fr.liris.plume/plume.sample.unity.Frame";
        public FixedString128Bytes TypeUrl => FrameSampleTypeUrl;

        public readonly int FrameNumber;
        public NativeArray<Any> Data;

        public FrameSample(int frameNumber, NativeArray<Any> data)
        {
            FrameNumber = frameNumber;
            Data = data;
        }

        public static FrameSample Pack(Allocator allocator, int frameNumber,
            ref SampleTypeUrlRegistry typeUrlRegistry, ref SerializedSamplesBuffer buffer)
        {
            var data = new NativeArray<Any>(buffer.ChunkCount, allocator);

            var offset = 0;

            for (var chunkIdx = 0; chunkIdx < buffer.ChunkCount; chunkIdx++)
            {
                var chunkLength = buffer.GetLength(chunkIdx);
                var chunkData = buffer.GetData(allocator, offset, chunkLength);
                var chunkSampleTypeUrlIndex = buffer.GetSampleTypeUrlIndex(chunkIdx);
                data[chunkIdx] = new Any(chunkData, typeUrlRegistry.GetTypeUrlFromIndex(chunkSampleTypeUrlIndex));
                offset += chunkLength;
            }

            return new FrameSample(frameNumber, data);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteToNoResize(ref NativeList<byte> data)
        {
            WritingPrimitives.WriteTagNoResize(Frame.FrameNumberFieldNumber, WireFormat.WireType.VarInt,
                ref data);
            WritingPrimitives.WriteInt32NoResize(FrameNumber, ref data);

            foreach (var frameData in Data)
            {
                var fd = frameData;
                WritingPrimitives.WriteTagNoResize(Frame.DataFieldNumber,
                    WireFormat.WireType.LengthDelimited, ref data);
                WritingPrimitives.WriteLengthPrefixedMessageNoResize(ref fd, ref data);
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ComputeMaxSize()
        {
            var size = WritingPrimitives.TagSize + WritingPrimitives.Int32MaxSize;

            foreach (var frameData in Data)
            {
                size += WritingPrimitives.TagSize + WritingPrimitives.LengthPrefixMaxSize;
                size += frameData.ComputeMaxSize();
            }

            return size;
        }

        public void Dispose()
        {
            foreach (var data in Data)
            {
                data.Dispose();
            }

            Data.Dispose();
        }
    }
}