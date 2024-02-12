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
        private static readonly FixedString128Bytes PackedSampleTypeUrl = "fr.liris.plume/fr.liris.plume.PackedSample";

        public FixedString128Bytes TypeUrl => PackedSampleTypeUrl;

        private readonly bool _hasTimestamp;
        private readonly long _timestamp;
        private Any _payload;

        private PackedSample(long timestamp, Any payload)
        {
            _hasTimestamp = true;
            _timestamp = timestamp;
            _payload = payload;
        }

        public static PackedSample Pack<T>(Allocator allocator, long timestamp, T message) where T : struct, IProtoBurstMessage
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
            WritingPrimitives.WriteMessage(ref _payload, ref data);
        }

        public void Dispose()
        {
            _payload.Dispose();
        }
    }
}