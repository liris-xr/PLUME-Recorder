using System;
using Google.Protobuf;
using PLUME.Core.Recorder.ProtoBurst;
using ProtoBurst;
using Unity.Collections;

namespace PLUME.Core.Recorder.Data
{
    public class RecordDataBuffer : IDisposable
    {
        private readonly object _lock = new();

        private DataChunks _timelessDataChunks;
        private TimestampedDataChunks _timestampedDataChunks;

        public RecordDataBuffer(Allocator allocator)
        {
            _timelessDataChunks = new DataChunks(allocator);
            _timestampedDataChunks = new TimestampedDataChunks(allocator);
        }

        public RecordDataBuffer(TimestampedDataChunks timestampedDataChunks, DataChunks timelessDataChunks)
        {
            _timestampedDataChunks = timestampedDataChunks;
            _timelessDataChunks = timelessDataChunks;
        }

        public void AddTimelessSample(IMessage msg)
        {
            var packedSample = PackedSample.Pack(msg, Allocator.Persistent);
            var bytes = packedSample.SerializeLengthPrefixed(Allocator.Persistent);

            lock (_lock)
            {
                _timelessDataChunks.Add(bytes);
            }

            bytes.Dispose();
            packedSample.Dispose();
        }

        public void AddTimestampedSample(IMessage msg, long timestamp)
        {
            var packedSample = PackedSample.Pack(timestamp, msg, Allocator.Persistent);
            var bytes = packedSample.SerializeLengthPrefixed(Allocator.Persistent);

            lock (_lock)
            {
                _timestampedDataChunks.Add(bytes, timestamp);
            }

            bytes.Dispose();
            packedSample.Dispose();
        }

        public void AddTimelessSample<T>(T msg) where T : unmanaged, IProtoBurstMessage
        {
            var packedSample = PackedSample.Pack(msg, Allocator.Persistent);
            var bytes = packedSample.SerializeLengthPrefixed(Allocator.Persistent);

            lock (_lock)
            {
                _timelessDataChunks.Add(bytes);
            }

            bytes.Dispose();
            packedSample.Dispose();
        }

        public void AddTimestampedSample<T>(T msg, long timestamp) where T : unmanaged, IProtoBurstMessage
        {
            var packedSample = PackedSample.Pack(timestamp, msg, Allocator.Persistent);
            var bytes = packedSample.SerializeLengthPrefixed(Allocator.Persistent);

            lock (_lock)
            {
                _timestampedDataChunks.Add(bytes, timestamp);
            }

            bytes.Dispose();
            packedSample.Dispose();
        }

        public bool TryRemoveAllTimelessDataChunks(DataChunks dst)
        {
            lock (_lock)
            {
                return _timelessDataChunks.TryRemoveAll(dst);
            }
        }

        public bool TryRemoveAllTimestampedDataChunks(TimestampedDataChunks dst)
        {
            lock (_lock)
            {
                return _timestampedDataChunks.TryRemoveAll(dst);
            }
        }

        public bool TryRemoveAllTimestampedDataChunksBeforeTimestamp(long timestamp, TimestampedDataChunks dst,
            bool inclusive)
        {
            lock (_lock)
            {
                return _timestampedDataChunks.TryRemoveAllBeforeTimestamp(timestamp, dst, inclusive);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _timelessDataChunks.Clear();
                _timestampedDataChunks.Clear();
            }
        }

        public void Dispose()
        {
            _timelessDataChunks.Dispose();
            _timestampedDataChunks.Dispose();
        }
    }
}