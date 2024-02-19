using System;
using PLUME.Core.Recorder.Data;
using ProtoBurst;
using ProtoBurst.Message;
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
        public DataChunks SerializedSamplesData;
        public DataChunks SerializedSamplesTypeUrl;

        public static readonly uint FrameNumberFieldTag = WireFormat.MakeTag(1, WireFormat.WireType.VarInt);
        public static readonly uint DataFieldTag = WireFormat.MakeTag(2, WireFormat.WireType.LengthDelimited);

        public FrameSample(int frameNumber, DataChunks serializedSamplesData, DataChunks serializedSamplesTypeUrl)
        {
            FrameNumber = frameNumber;

            if (serializedSamplesData.ChunksCount != serializedSamplesTypeUrl.ChunksCount)
                throw new ArgumentException(
                    $"{nameof(serializedSamplesData)} and {nameof(serializedSamplesTypeUrl)} must have the same number of chunks.");

            SerializedSamplesData = serializedSamplesData;
            SerializedSamplesTypeUrl = serializedSamplesTypeUrl;
        }

        public void WriteTo(ref BufferWriter bufferWriter)
        {
            bufferWriter.WriteTag(FrameNumberFieldTag);
            bufferWriter.WriteInt32(FrameNumber);

            var serializedSampleData = SerializedSamplesData.GetDataSpan();
            var serializedSampleTypeUrl = SerializedSamplesTypeUrl.GetDataSpan();
            var valueByteOffset = 0;
            var typeUrlByteOffset = 0;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var chunkIdx = 0; chunkIdx < SerializedSamplesData.ChunksCount; chunkIdx++)
            {
                var valueBytesLength = SerializedSamplesData.GetLength(chunkIdx);
                var typeUrlBytesLength = SerializedSamplesTypeUrl.GetLength(chunkIdx);
                var valueBytes = serializedSampleData.Slice(valueByteOffset, valueBytesLength);
                var typeUrlBytes = serializedSampleTypeUrl.Slice(typeUrlByteOffset, typeUrlBytesLength);

                unsafe
                {
                    fixed (byte* valueBytesPtr = valueBytes)
                    {
                        fixed (byte* typeUrlBytesPtr = typeUrlBytes)
                        {
                            WriteDataChunkTo(typeUrlBytesPtr, typeUrlBytesLength, valueBytesPtr, valueBytesLength,
                                ref bufferWriter);
                        }
                    }
                }

                valueByteOffset += valueBytesLength;
                typeUrlByteOffset += typeUrlBytesLength;
            }
        }

        public int ComputeSize()
        {
            var size = 0;

            size += BufferExtensions.ComputeTagSize(FrameNumberFieldTag) +
                    BufferExtensions.ComputeInt32Size(FrameNumber);

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var chunkIdx = 0; chunkIdx < SerializedSamplesData.ChunksCount; chunkIdx++)
            {
                var valueBytesLength = SerializedSamplesData.GetLength(chunkIdx);
                var typeUrlBytesLength = SerializedSamplesTypeUrl.GetLength(chunkIdx);
                size += ComputeDataChunkSize(valueBytesLength, typeUrlBytesLength);
            }

            return size;
        }

        internal static unsafe void WriteDataChunkTo(
            byte* typeUrlBytesPtr, int typeUrlBytesLength,
            byte* valueBytesPtr, int valueBytesLength,
            ref BufferWriter bufferWriter)
        {
            bufferWriter.WriteTag(DataFieldTag);
            bufferWriter.WriteLength(Any.ComputeSize(typeUrlBytesLength, valueBytesLength));
            Any.WriteTo(typeUrlBytesPtr, typeUrlBytesLength, valueBytesPtr, valueBytesLength, ref bufferWriter);
        }

        internal static int ComputeDataChunkSize(int valueBytesLength, int typeUrlBytesLength)
        {
            var anySize = Any.ComputeSize(typeUrlBytesLength, valueBytesLength);

            return BufferExtensions.ComputeTagSize(DataFieldTag) +
                   BufferExtensions.ComputeLengthPrefixSize(anySize) + anySize;
        }

        public SampleTypeUrl GetTypeUrl(Allocator allocator)
        {
            return SampleTypeUrl.Alloc(TypeUrl, allocator);
        }

        public void Dispose()
        {
            SerializedSamplesData.Dispose();
            SerializedSamplesTypeUrl.Dispose();
        }
    }
}