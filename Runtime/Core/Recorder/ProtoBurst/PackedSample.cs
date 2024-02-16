using System;
using System.Runtime.CompilerServices;
using ProtoBurst;
using ProtoBurst.Message;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Core.Recorder.ProtoBurst
{
    [BurstCompile]
    public struct PackedSample : IProtoBurstMessage, IDisposable
    {
        public FixedString128Bytes TypeUrl => "fr.liris.plume/plume.sample.PackedSample";

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

        public static PackedSample Pack<T>(Allocator allocator, T message) where T : unmanaged, IProtoBurstMessage
        {
            return new PackedSample(Any.Pack(allocator, message));
        }

        public static PackedSample Pack<T>(Allocator allocator, long timestamp, T message)
            where T : unmanaged, IProtoBurstMessage
        {
            return new PackedSample(timestamp, Any.Pack(allocator, message));
        }
        
        public static PackedSample Pack(NativeArray<byte> msgBytes, FixedString128Bytes msgTypeUrl)
        {
            return new PackedSample(new Any(msgBytes, msgTypeUrl));
        }
        
        public static PackedSample Pack(long timestamp, NativeArray<byte> msgBytes, FixedString128Bytes msgTypeUrl)
        {
            return new PackedSample(timestamp, new Any(msgBytes, msgTypeUrl));
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