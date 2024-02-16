using System;
using System.Collections.Generic;

namespace PLUME.Core.Recorder.Data
{
    public class RecordData : IRecordData
    {
        internal readonly TimelessDataChunks TimelessDataChunks;
        internal readonly TimestampedDataChunks TimestampedDataChunks;

        public RecordData()
        {
            TimelessDataChunks = new TimelessDataChunks();
            TimestampedDataChunks = new TimestampedDataChunks();
        }

        public RecordData(TimestampedDataChunks timestampedDataChunks, TimelessDataChunks timelessDataChunks)
        {
            TimestampedDataChunks = timestampedDataChunks;
            TimelessDataChunks = timelessDataChunks;
        }

        internal RecordData Copy()
        {
            var timestampedDataChunksCopy = TimestampedDataChunks.Copy();
            var timelessDataChunksCopy = TimelessDataChunks.Copy();
            var copy = new RecordData(timestampedDataChunksCopy, timelessDataChunksCopy);
            return copy;
        }

        public void AddTimelessDataChunk(ReadOnlySpan<byte> data)
        {
            TimelessDataChunks.Push(data);
        }

        public void AddTimestampedDataChunk(ReadOnlySpan<byte> data, long timestamp)
        {
            TimestampedDataChunks.Push(data, timestamp);
        }

        public bool TryPopAllTimelessDataChunks(DataChunks dataChunks)
        {
            return TimelessDataChunks.TryPopAll(dataChunks);
        }

        public bool TryPopAllTimestampedDataChunks(DataChunks dataChunks, List<long> timestamps)
        {
            return TimestampedDataChunks.TryPopAll(dataChunks, timestamps);
        }

        public bool TryPopTimestampedDataChunksBefore(long timestamp, DataChunks dataChunks, List<long> timestamps,
            bool inclusive)
        {
            return TimestampedDataChunks.TryPopAllBeforeTimestamp(timestamp, dataChunks, timestamps, inclusive);
        }

        public void Clear()
        {
            TimelessDataChunks.Clear();
            TimestampedDataChunks.Clear();
        }
    }
}