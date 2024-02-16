using System;
using System.Runtime.CompilerServices;
using Google.Protobuf;
using ProtoBurst;
using ProtoBurst.Message;
using Unity.Burst;
using Unity.Collections;
using WireFormat = ProtoBurst.WireFormat;

namespace PLUME.Core.Recorder.ProtoBurst
{
    [BurstCompile]
    public struct PackedSample : IProtoBurstMessage, IDisposable
    {
        public static readonly FixedString128Bytes SampleTypeUrl = "fr.liris.plume/" + Sample.PackedSample.Descriptor.FullName;
        public FixedString128Bytes TypeUrl => SampleTypeUrl;

        private bool _hasTimestamp;
        private long _timestamp;

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

        public static PackedSample PackManaged<T>(long timestamp, T msg, Allocator allocator)
            where T : IMessage
        {
            return new PackedSample(timestamp, Any.PackManaged(msg, allocator));
        }
        
        public static PackedSample PackManaged<T>(T msg, Allocator allocator)
            where T : IMessage
        {
            return new PackedSample(Any.PackManaged(msg, allocator));
        }

        public static PackedSample Pack(NativeArray<byte> msgBytes, FixedString128Bytes msgTypeUrl)
        {
            return new PackedSample(Any.Pack(msgBytes, msgTypeUrl));
        }

        public static PackedSample Pack(long timestamp, NativeArray<byte> msgBytes, FixedString128Bytes msgTypeUrl)
        {
            return new PackedSample(timestamp, Any.Pack(msgBytes, msgTypeUrl));
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteToNoResize(ref NativeList<byte> data)
        {
            if (_hasTimestamp)
            {
                WritingPrimitives.WriteTagNoResize(Sample.PackedSample.TimestampFieldNumber, WireFormat.WireType.VarInt,
                    ref data);
                WritingPrimitives.WriteInt64NoResize(_timestamp, ref data);
            }

            WritingPrimitives.WriteTagNoResize(Sample.PackedSample.PayloadFieldNumber,
                WireFormat.WireType.LengthDelimited, ref data);
            WritingPrimitives.WriteLengthPrefixedMessageNoResize(ref Payload, ref data);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ComputeMaxSize()
        {
            var size = 0;

            if (_hasTimestamp)
            {
                size += WritingPrimitives.TagSize + WritingPrimitives.Int64MaxSize;
            }

            size += WritingPrimitives.TagSize + WritingPrimitives.LengthPrefixMaxSize + Payload.ComputeMaxSize();
            return size;
        }

        public void Dispose()
        {
            Payload.Dispose();
        }
    }
}