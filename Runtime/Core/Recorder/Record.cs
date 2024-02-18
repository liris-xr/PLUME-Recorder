using System;
using Google.Protobuf;
using PLUME.Core.Recorder.Data;
using PLUME.Core.Recorder.ProtoBurst;
using PLUME.Core.Recorder.Time;
using ProtoBurst;
using Unity.Collections;
using UnityEngine;

namespace PLUME.Core.Recorder
{
    public class Record : IDisposable
    {
        public IReadOnlyClock Clock => InternalClock;
        public readonly RecordIdentifier Identifier;
        
        internal readonly Clock InternalClock;
        
        private DataChunks _timelessDataBuffer;
        private TimestampedDataChunks _timestampedDataBuffer;
        private readonly object _timelessDataBufferLock = new();
        private readonly object _timestampedDataBufferLock = new();

        internal Record(Clock clock, RecordIdentifier identifier, Allocator allocator)
        {
            InternalClock = clock;
            Identifier = identifier;
            _timelessDataBuffer = new DataChunks(allocator);
            _timestampedDataBuffer = new TimestampedDataChunks(allocator);
        }
        
        public void RecordTimelessSample(IMessage msg)
        {
            var packedSample = PackedSample.Pack(msg, Allocator.Persistent);
            var bytes = packedSample.SerializeLengthPrefixed(Allocator.Persistent);

            lock (_timelessDataBufferLock)
            {
                _timelessDataBuffer.Add(bytes.AsArray());
            }

            bytes.Dispose();
            packedSample.Dispose();
        }

        public void RecordTimestampedSample(IMessage msg, long timestamp)
        {
            var packedSample = PackedSample.Pack(timestamp, msg, Allocator.Persistent);
            var bytes = packedSample.SerializeLengthPrefixed(Allocator.Persistent);

            lock (_timestampedDataBufferLock)
            {
                _timestampedDataBuffer.Add(bytes.AsArray(), timestamp);
            }

            bytes.Dispose();
            packedSample.Dispose();
        }

        public void RecordTimelessSample<T>(T msg) where T : unmanaged, IProtoBurstMessage
        {
            var packedSample = PackedSample.Pack(msg, Allocator.Persistent);
            var bytes = packedSample.SerializeLengthPrefixed(Allocator.Persistent);

            lock (_timelessDataBufferLock)
            {
                _timelessDataBuffer.Add(bytes.AsArray());
            }

            bytes.Dispose();
            packedSample.Dispose();
        }

        public void RecordTimestampedSample<T>(T msg, long timestamp) where T : unmanaged, IProtoBurstMessage
        {
            var packedSample = PackedSample.Pack(timestamp, msg, Allocator.Persistent);
            var bytes = packedSample.SerializeLengthPrefixed(Allocator.Persistent);

            lock (_timestampedDataBufferLock)
            {
                _timestampedDataBuffer.Add(bytes.AsArray(), timestamp);
            }

            bytes.Dispose();
            packedSample.Dispose();
        }

        internal bool TryRemoveAllTimelessDataChunks(DataChunks dst)
        {
            lock (_timelessDataBufferLock)
            {
                return _timelessDataBuffer.TryRemoveAll(dst);
            }
        }

        internal bool TryRemoveAllTimestampedDataChunks(TimestampedDataChunks dst)
        {
            lock (_timestampedDataBufferLock)
            {
                return _timestampedDataBuffer.TryRemoveAll(dst);
            }
        }

        internal bool TryRemoveAllTimestampedDataChunksBeforeTimestamp(long timestamp, TimestampedDataChunks dst,
            bool inclusive)
        {
            lock (_timestampedDataBufferLock)
            {
                return _timestampedDataBuffer.TryRemoveAllBeforeTimestamp(timestamp, dst, inclusive);
            }
        }

        public void Dispose()
        {
            _timelessDataBuffer.Dispose();
            _timestampedDataBuffer.Dispose();
        }
    }
}