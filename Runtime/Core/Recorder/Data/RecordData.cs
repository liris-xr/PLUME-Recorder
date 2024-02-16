using System.Collections.Generic;
using Google.Protobuf;
using PLUME.Core.Recorder.ProtoBurst;
using ProtoBurst;
using Unity.Collections;

namespace PLUME.Core.Recorder.Data
{
    public class RecordData : IRecordData
    {
        private readonly TimelessDataChunks _timelessDataChunks;
        private readonly TimestampedDataChunks _timestampedDataChunks;

        public RecordData()
        {
            _timelessDataChunks = new TimelessDataChunks();
            _timestampedDataChunks = new TimestampedDataChunks();
        }

        public RecordData(TimestampedDataChunks timestampedDataChunks, TimelessDataChunks timelessDataChunks)
        {
            _timestampedDataChunks = timestampedDataChunks;
            _timelessDataChunks = timelessDataChunks;
        }

        public void PushTimelessSample(IMessage msg)
        {
            var packedSample = PackedSample.PackManaged(msg, Allocator.Persistent);
            var bytes = packedSample.SerializeLengthPrefixed(Allocator.Persistent);
            _timelessDataChunks.Push(bytes);
            bytes.Dispose();
            packedSample.Dispose();
        }

        public void PushTimestampedSample(IMessage msg, long timestamp)
        {
            var packedSample = PackedSample.PackManaged(timestamp, msg, Allocator.Persistent);
            var bytes = packedSample.SerializeLengthPrefixed(Allocator.Persistent);
            _timestampedDataChunks.Push(bytes, timestamp);
            bytes.Dispose();
            packedSample.Dispose();
        }

        public void PushTimelessSample<T>(T msg) where T : unmanaged, IProtoBurstMessage
        {
            var packedSample = PackedSample.Pack(msg, Allocator.Persistent);
            var bytes = packedSample.SerializeLengthPrefixed(Allocator.Persistent);
            _timelessDataChunks.Push(bytes);
            bytes.Dispose();
            packedSample.Dispose();
        }

        public void PushTimestampedSample<T>(T msg, long timestamp) where T : unmanaged, IProtoBurstMessage
        {
            var packedSample = PackedSample.Pack(timestamp, msg, Allocator.Persistent);
            var bytes = packedSample.SerializeLengthPrefixed(Allocator.Persistent);
            _timestampedDataChunks.Push(bytes, timestamp);
            bytes.Dispose();
            packedSample.Dispose();
        }

        public void PushTimelessSample(NativeArray<byte> sampleData, FixedString128Bytes sampleTypeUrl)
        {
            var packedSample = PackedSample.Pack(sampleData, sampleTypeUrl);
            var bytes = packedSample.SerializeLengthPrefixed(Allocator.Persistent);
            _timelessDataChunks.Push(bytes);
            bytes.Dispose();
            packedSample.Dispose();
        }

        public void PushTimestampedSample(NativeArray<byte> sampleData, FixedString128Bytes sampleTypeUrl,
            long timestamp)
        {
            var packedSample = PackedSample.Pack(timestamp, sampleData, sampleTypeUrl);
            var bytes = packedSample.SerializeLengthPrefixed(Allocator.Persistent);
            _timestampedDataChunks.Push(bytes, timestamp);
            bytes.Dispose();
            packedSample.Dispose();
        }

        public bool TryPopAllTimelessDataChunks(DataChunks dataChunks)
        {
            return _timelessDataChunks.TryPopAll(dataChunks);
        }

        public bool TryPopAllTimestampedDataChunks(DataChunks dataChunks, List<long> timestamps)
        {
            return _timestampedDataChunks.TryPopAll(dataChunks, timestamps);
        }

        public bool TryPopTimestampedDataChunksBefore(long timestamp, DataChunks dataChunks, List<long> timestamps,
            bool inclusive)
        {
            return _timestampedDataChunks.TryPopAllBeforeTimestamp(timestamp, dataChunks, timestamps, inclusive);
        }

        public void Clear()
        {
            _timelessDataChunks.Clear();
            _timestampedDataChunks.Clear();
        }
    }
}