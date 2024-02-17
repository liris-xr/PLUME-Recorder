using System;
using System.Runtime.CompilerServices;
using PLUME.Core.Recorder.Data;
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
        public static readonly FixedString128Bytes SampleTypeUrl = "fr.liris.plume/" + Frame.Descriptor.FullName;

        public FixedString128Bytes TypeUrl => SampleTypeUrl;

        public readonly int FrameNumber;
        public NativeArray<Any> Data;

        public FrameSample(int frameNumber, NativeArray<Any> data)
        {
            FrameNumber = frameNumber;
            Data = data;
        }

        public static FrameSample Pack(int frameNumber, ref SampleTypeUrlRegistry typeUrlRegistry,
            ref FrameDataChunks frameDataSamples, Allocator allocator)
        {
            var data = new NativeArray<Any>(frameDataSamples.ChunksCount, allocator);
            
            for (var chunkIdx = 0; chunkIdx < frameDataSamples.ChunksCount; chunkIdx++)
            {
                var chunkData = frameDataSamples.GetChunkData(chunkIdx);
                var chunkSampleTypeUrlIndex = frameDataSamples.GetSampleTypeUrlIndex(chunkIdx);
                data[chunkIdx] = Any.Pack(chunkData, typeUrlRegistry.GetTypeUrlFromIndex(chunkSampleTypeUrlIndex), allocator);
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
            foreach (var frameData in Data)
            {
                frameData.Dispose();
            }

            Data.Dispose();
        }
    }
}