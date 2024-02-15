using System;
using System.Collections.Generic;

namespace PLUME.Core.Recorder.Data
{
    public class ConcurrentRecordData : IRecordData
    {
        private readonly object _lock = new();
        private readonly RecordData _data = new();
        
        public void AddTimelessDataChunk(ReadOnlySpan<byte> data)
        {
            lock (_lock)
            {
                _data.AddTimelessDataChunk(data);
            }
        }

        public void AddTimestampedDataChunk(ReadOnlySpan<byte> data, long timestamp)
        {
            lock (_lock)
            {
                _data.AddTimestampedDataChunk(data, timestamp);
            }
        }

        public bool TryPopAllTimelessDataChunks(DataChunks dataChunks)
        {
            lock (_lock)
            {
                return _data.TryPopAllTimelessDataChunks(dataChunks);
            }
        }

        public bool TryPopAllTimestampedDataChunks(DataChunks dataChunks, List<long> chunksTimestamp)
        {
            lock (_lock)
            {
                return _data.TryPopAllTimestampedDataChunks(dataChunks, chunksTimestamp);
            }
        }

        public bool TryPopTimestampedDataChunksBefore(long timestamp, DataChunks dataChunks, List<long> chunksTimestamp, bool inclusive)
        {
            lock (_lock)
            {
                return _data.TryPopTimestampedDataChunksBefore(timestamp, dataChunks, chunksTimestamp, inclusive);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _data.Clear();
            }
        }
    }
}