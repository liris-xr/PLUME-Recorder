using System;
using PLUME.Core.Collections;
using Unity.Collections;

namespace PLUME.Core.Recorder
{
    public struct NativeRecorderData : IRecorderData, IDisposable
    {
        private NativeDataChunks _timelessData;
        private NativeTimestampedDataChunks _timestampedData;
        
        public NativeRecorderData(Allocator allocator)
        {
            _timelessData = new NativeDataChunks(allocator);
            _timestampedData = new NativeTimestampedDataChunks(allocator);
        }
        
        public void AddTimelessData(ReadOnlySpan<byte> data)
        {
            _timelessData.Add(data);
        }
        
        public void AddTimelessData(ReadOnlySpan<byte> data, ReadOnlySpan<int> lengths)
        {
            _timelessData.Add(data, lengths);
        }
        
        public void AddTimestampedData(ReadOnlySpan<byte> data, long timestamp)
        {
            _timestampedData.Enqueue(data, timestamp);
        }
        
        public void AddTimestampedData(ReadOnlySpan<byte> data, ReadOnlySpan<int> lengths, ReadOnlySpan<long> timestamps)
        {
            _timestampedData.Enqueue(data, lengths, timestamps);
        }
        
        public void Dispose()
        {
            _timelessData.Dispose();
            _timestampedData.Dispose();
        }
    }
}