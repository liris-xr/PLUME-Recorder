using System;
using System.Collections.Generic;

namespace PLUME.Core.Recorder.Data
{
    public class RecordData : IRecordData
    {
        private readonly DataChunks _timelessDataChunks = new();

        private readonly DataChunks _timestampedDataChunks = new();
        private readonly List<long> _chunksTimestamps = new();

        public void AddTimelessDataChunk(ReadOnlySpan<byte> data)
        {
            _timelessDataChunks.AddDataChunk(data);
        }

        public void AddTimestampedDataChunk(ReadOnlySpan<byte> data, long timestamp)
        {
            var chunkIndex = _chunksTimestamps.BinarySearch(timestamp);

            if (chunkIndex >= 0)
            {
                _timestampedDataChunks.MergeIntoChunk(chunkIndex, data, DataChunks.ChunkPosition.End);
                return;
            }

            // In this case, chunk index is the bitwise complement of the index of the next element that is larger
            // than timestamp or, if there is no larger element,the bitwise complement of Count.
            _timestampedDataChunks.InsertDataChunk(~chunkIndex, data);
            _chunksTimestamps.Insert(~chunkIndex, timestamp);
        }
        
        public bool TryPopAllTimelessDataChunks(DataChunks dataChunks)
        {
            dataChunks.Clear();
            
            if(_timelessDataChunks.ChunksCount == 0)
                return false;
            
            var chunksData = _timelessDataChunks.GetAllChunksData();
            var chunksLengths = _timelessDataChunks.GetAllChunksDataLength();
            dataChunks.AddDataChunks(chunksData, chunksLengths);
            _timelessDataChunks.Clear();
            return true;
        }

        public bool TryPopAllTimestampedDataChunks(DataChunks dataChunks, List<long> chunksTimestamp)
        {
            dataChunks.Clear();
            chunksTimestamp.Clear();
            
            if(_timestampedDataChunks.ChunksCount == 0)
                return false;
            
            var chunksData = _timestampedDataChunks.GetAllChunksData();
            var chunksLengths = _timestampedDataChunks.GetAllChunksDataLength();
            dataChunks.AddDataChunks(chunksData, chunksLengths);
            chunksTimestamp.AddRange(_chunksTimestamps);
            _timestampedDataChunks.Clear();
            _chunksTimestamps.Clear();
            return true;
        }
        
        public bool TryPopTimestampedDataChunksBefore(long timestamp, DataChunks dataChunks, List<long> chunksTimestamp, bool inclusive)
        {
            dataChunks.Clear();
            chunksTimestamp.Clear();
            
            var chunkIndex = _chunksTimestamps.BinarySearch(timestamp);

            // Exact match, a chunk has the same timestamp
            if (chunkIndex >= 0)
            {
                // Nothing to pop
                if(chunkIndex == 0 && !inclusive)
                    return false;
            }
            else
            {
                chunkIndex = ~chunkIndex - 1;
                
                // Nothing to pop
                if(chunkIndex < 0)
                    return false;
            }
            
            // Pop from 0 to chunkIndex
            var chunksData = dataChunks.GetChunksData(0, chunkIndex + 1);
            var chunksLengths = dataChunks.GetChunksDataLength(0, chunkIndex + 1);
            dataChunks.AddDataChunks(chunksData, chunksLengths);
            dataChunks.RemoveDataChunks(0, chunkIndex + 1);
            
            chunksTimestamp.AddRange(_chunksTimestamps.GetRange(0, chunkIndex + 1));
            _chunksTimestamps.RemoveRange(0, chunkIndex + 1);
            return true;
        }
        
        public void Clear()
        {
            _timelessDataChunks.Clear();
            _timestampedDataChunks.Clear();
            _chunksTimestamps.Clear();
        }
    }
}