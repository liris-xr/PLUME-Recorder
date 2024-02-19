using System;
using ProtoBurst;
using ProtoBurst.Message;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Core.Recorder.ProtoBurst
{
    [BurstCompile]
    public struct PackedSample : IProtoBurstMessage, IDisposable
    {
        public static readonly FixedString128Bytes TypeUrl = "fr.liris.plume/plume.sample.PackedSample";

        private bool _hasTimestamp;
        private long _timestamp;

        public static readonly uint TimestampFieldTag = WireFormat.MakeTag(1, WireFormat.WireType.VarInt);
        public static readonly uint PayloadFieldTag = WireFormat.MakeTag(2, WireFormat.WireType.LengthDelimited);

        public long Timestamp
        {
            get => _timestamp;
            set
            {
                _hasTimestamp = true;
                _timestamp = value;
            }
        }

        public Any Payload;

        public PackedSample(Any payload)
        {
            _hasTimestamp = false;
            _timestamp = default;
            Payload = payload;
        }

        public PackedSample(long timestamp, Any payload)
        {
            _hasTimestamp = true;
            _timestamp = timestamp;
            Payload = payload;
        }

        public static PackedSample Pack<T>(T message, Allocator allocator) where T : unmanaged, IProtoBurstMessage
        {
            return new PackedSample(message.ToAny(allocator));
        }

        public static PackedSample Pack<T>(long timestamp, T message, Allocator allocator)
            where T : unmanaged, IProtoBurstMessage
        {
            return new PackedSample(timestamp, message.ToAny(allocator));
        }

        public void WriteTo(ref BufferWriter bufferWriter)
        {
            if (_hasTimestamp)
            {
                bufferWriter.WriteTag(TimestampFieldTag);
                bufferWriter.WriteInt64(_timestamp);
            }

            bufferWriter.WriteTag(PayloadFieldTag);
            bufferWriter.WriteLengthPrefixedMessage(ref Payload);
        }

        public int ComputeSize()
        {
            var size = 0;

            if (_hasTimestamp)
            {
                size += BufferExtensions.ComputeTagSize(TimestampFieldTag) +
                        BufferExtensions.ComputeInt64Size(_timestamp);
            }

            size += BufferExtensions.ComputeTagSize(PayloadFieldTag) +
                    BufferExtensions.ComputeLengthPrefixedMessageSize(ref Payload);

            return size;
        }

        public SampleTypeUrl GetTypeUrl(Allocator allocator)
        {
            return SampleTypeUrl.Alloc(TypeUrl, allocator);
        }

        public void Dispose()
        {
            Payload.Dispose();
        }
    }
}