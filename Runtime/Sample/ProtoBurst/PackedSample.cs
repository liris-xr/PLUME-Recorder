using System;
using ProtoBurst;
using ProtoBurst.Message;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Sample.ProtoBurst
{
    [BurstCompile]
    public struct PackedSample : IProtoBurstMessage, IDisposable
    {
        public static readonly FixedString128Bytes TypeUrl = "fr.liris.plume/plume.sample.PackedSample";

        public static readonly uint TimestampFieldTag =
            WireFormat.MakeTag(Sample.PackedSample.TimestampFieldNumber, WireFormat.WireType.VarInt);

        public static readonly uint PayloadFieldTag =
            WireFormat.MakeTag(Sample.PackedSample.PayloadFieldNumber, WireFormat.WireType.LengthDelimited);

        private readonly bool _hasTimestamp;
        private readonly ulong _timestamp;

        private NativeList<byte> _payloadRawBytes;

        private PackedSample(ulong timestamp, NativeList<byte> payloadRawBytes)
        {
            _hasTimestamp = true;
            _timestamp = timestamp;
            _payloadRawBytes = payloadRawBytes;
        }

        private PackedSample(NativeList<byte> payloadRawBytes)
        {
            _hasTimestamp = false;
            _timestamp = default;
            _payloadRawBytes = payloadRawBytes;
        }

        public static PackedSample Pack(ulong timestamp, ref Any payload, Allocator allocator)
        {
            var payloadRawBytes = payload.ToBytes(allocator);
            return new PackedSample(timestamp, payloadRawBytes);
        }

        public static PackedSample Pack(ulong timestamp,
            ref NativeList<byte> payloadValueBytes,
            ref NativeList<byte> payloadTypeUrlBytes,
            Allocator allocator)
        {
            var payload = Any.Pack(ref payloadValueBytes, ref payloadTypeUrlBytes, allocator);
            var payloadRawBytes = payload.ToBytes(allocator);
            payload.Dispose();
            return new PackedSample(timestamp, payloadRawBytes);
        }

        public static PackedSample Pack(
            ref NativeList<byte> payloadValueBytes,
            ref NativeList<byte> payloadTypeUrlBytes,
            Allocator allocator)
        {
            var payload = Any.Pack(ref payloadValueBytes, ref payloadTypeUrlBytes, allocator);
            var payloadRawBytes = payload.ToBytes(allocator);
            payload.Dispose();
            return new PackedSample(payloadRawBytes);
        }

        public static PackedSample Pack<T>(ulong timestamp, ref T message, Allocator allocator)
            where T : unmanaged, IProtoBurstMessage
        {
            var payload = Any.Pack(ref message, allocator);
            var payloadRawBytes = payload.ToBytes(allocator);
            payload.Dispose();
            return new PackedSample(timestamp, payloadRawBytes);
        }

        public void WriteTo(ref BufferWriter bufferWriter)
        {
            if (_hasTimestamp)
            {
                bufferWriter.WriteTag(TimestampFieldTag);
                bufferWriter.WriteUInt64(_timestamp);
            }

            bufferWriter.WriteTag(PayloadFieldTag);
            bufferWriter.WriteLengthPrefixedBytes(ref _payloadRawBytes);
        }

        public int ComputeSize()
        {
            var size = 0;

            if (_hasTimestamp)
            {
                size += BufferWriterExtensions.ComputeTagSize(TimestampFieldTag) +
                        BufferWriterExtensions.ComputeUInt64Size(_timestamp);
            }

            size += BufferWriterExtensions.ComputeTagSize(PayloadFieldTag) +
                    BufferWriterExtensions.ComputeLengthPrefixedBytesSize(ref _payloadRawBytes);

            return size;
        }

        public SampleTypeUrl GetTypeUrl(Allocator allocator)
        {
            return SampleTypeUrl.Alloc(TypeUrl, allocator);
        }

        public void Dispose()
        {
            _payloadRawBytes.Dispose();
        }
    }
}