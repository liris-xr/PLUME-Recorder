using System;
using System.Collections.Generic;

namespace PLUME.Core.Recorder.Data
{
    public interface IRecordData
    {
        public void AddTimelessDataChunk(ReadOnlySpan<byte> data);
        
        public void AddTimestampedDataChunk(ReadOnlySpan<byte> data, long timestamp);
        
        public bool TryPopAllTimelessDataChunks(DataChunks dataChunks);
        
        public bool TryPopAllTimestampedDataChunks(DataChunks dataChunks, List<long> timestamps);
        
        public bool TryPopTimestampedDataChunksBefore(long timestamp, DataChunks dataChunks, List<long> timestamps, bool inclusive);
        
        public void Clear();
    }
}