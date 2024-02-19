using System;
using System.Runtime.CompilerServices;
using Google.Protobuf;
using ProtoBurst;
using ProtoBurst.Message;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Burst;
using Unity.Collections;
using WireFormat = ProtoBurst.WireFormat;

namespace PLUME.Core.Recorder.ProtoBurst
{
    [BurstCompile]
    public struct PackedSample : IProtoBurstMessage, IDisposable
    {
        public static readonly FixedString128Bytes TypeUrl = "fr.liris.plume/plume.sample.PackedSample";

        private bool _hasTimestamp;
        private long _timestamp;

        private static readonly uint TimestampFieldTag = WireFormat.MakeTag(1, WireFormat.WireType.VarInt);
        private static readonly uint PayloadFieldTag = WireFormat.MakeTag(2, WireFormat.WireType.LengthDelimited);

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
            return new PackedSample(Any.Pack(message, allocator));
        }

        public static PackedSample Pack<T>(long timestamp, T msg, Allocator allocator)
            where T : unmanaged, IProtoBurstMessage
        {
            return new PackedSample(timestamp, Any.Pack(msg, allocator));
        }

        [BurstDiscard]
        public static PackedSample Pack(long timestamp, IMessage msg, Allocator allocator)
        {
            return new PackedSample(timestamp, Any.Pack(msg, allocator));
        }

        public static PackedSample Pack(IMessage msg, Allocator allocator)
        {
            return new PackedSample(Any.Pack(msg, allocator));
        }

        public static PackedSample Pack(NativeArray<byte> value, SampleTypeUrl typeUrl)
        {
            return new PackedSample(Any.Pack(value, typeUrl));
        }

        public static PackedSample Pack(long timestamp, NativeArray<byte> value, SampleTypeUrl typeUrl)
        {
            return new PackedSample(timestamp, Any.Pack(value, typeUrl));
        }

        public void WriteTo(ref BufferWriter bufferWriter)
        {
            if (_hasTimestamp)
            {
                bufferWriter.WriteTag(TimestampFieldTag);
                bufferWriter.WriteSInt64(_timestamp);
            }

            bufferWriter.WriteTag(PayloadFieldTag);
            bufferWriter.WriteLengthPrefixedMessage(ref Payload);
        }
        
        public static void WriteTo<T>(long timestamp, ref T payload, ref BufferWriter bufferWriter) where T : unmanaged, IProtoBurstMessage
        {
            bufferWriter.WriteTag(TimestampFieldTag);
            bufferWriter.WriteSInt64(timestamp);
            bufferWriter.WriteTag(PayloadFieldTag);
            bufferWriter.WriteLengthPrefixedMessage(ref payload);
        }
        
        public static void WriteTo<T>(ref T payload, ref BufferWriter bufferWriter) where T : unmanaged, IProtoBurstMessage
        {
            bufferWriter.WriteTag(PayloadFieldTag);
            bufferWriter.WriteLengthPrefixedMessage(ref payload);
        }

        public static int ComputeSize<T>(long timestamp, ref T payload) where T : unmanaged, IProtoBurstMessage
        {
            var size = 0;
            size += BufferExtensions.TagSize + BufferExtensions.ComputeVarIntSize(timestamp);
            size += BufferExtensions.TagSize + BufferExtensions.ComputeLengthPrefixedMessageSize(ref payload);
            return size;
        }
        
        public static int ComputeSize<T>(ref T payload) where T : unmanaged, IProtoBurstMessage
        {
            return BufferExtensions.TagSize + BufferExtensions.ComputeLengthPrefixedMessageSize(ref payload);
        }

        public int ComputeSize()
        {
            var size = 0;

            if (_hasTimestamp)
            {
                size += BufferExtensions.TagSize + BufferExtensions.ComputeVarIntSize(_timestamp);
            }

            size += BufferExtensions.TagSize + BufferExtensions.ComputeLengthPrefixedMessageSize(ref Payload);
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