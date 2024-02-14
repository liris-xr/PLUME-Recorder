using System;
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

        private PackedSample(Any payload)
        {
            _hasTimestamp = false;
            _timestamp = default;
            Payload = payload;
        }

        private PackedSample(long timestamp, Any payload)
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

        [BurstCompile]
        public void WriteTo(ref NativeList<byte> data)
        {
            if (_hasTimestamp)
            {
                WritingPrimitives.WriteTag(Sample.PackedSample.TimestampFieldNumber, WireFormat.WireType.VarInt,
                    ref data);
                WritingPrimitives.WriteInt64(_timestamp, ref data);
            }

            WritingPrimitives.WriteTag(Sample.PackedSample.PayloadFieldNumber,
                WireFormat.WireType.LengthDelimited, ref data);
            WritingPrimitives.WriteMessage(ref Payload, ref data);
        }

        public void Dispose()
        {
            Payload.Dispose();
        }
    }
}