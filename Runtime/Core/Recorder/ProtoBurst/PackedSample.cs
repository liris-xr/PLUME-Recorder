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
        public static readonly SampleTypeUrl SampleTypeUrl =
            SampleTypeUrlRegistry.GetOrCreate("fr.liris.plume", Sample.PackedSample.Descriptor);

        public SampleTypeUrl TypeUrl => SampleTypeUrl;

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
        public void WriteTo(ref NativeList<byte> data)
        {
            if (_hasTimestamp)
            {
                WritingPrimitives.WriteTag(Sample.PackedSample.TimestampFieldNumber, WireFormat.WireType.VarInt,
                    ref data);
                WritingPrimitives.WriteInt64(_timestamp, ref data);
            }

            WritingPrimitives.WriteTag(Sample.PackedSample.PayloadFieldNumber, WireFormat.WireType.LengthDelimited,
                ref data);
            WritingPrimitives.WriteLengthPrefixedMessage(ref Payload, ref data);
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