using System;
using Unity.Collections;

namespace PLUME.Core.Recorder
{
    public class ConcurrentRecorderData : IRecorderData, IDisposable
    {
        private NativeRecorderData _data;
        private readonly object _lock = new();
        
        public ConcurrentRecorderData(Allocator allocator)
        {
            _data = new NativeRecorderData(allocator);
        }
        
        public void AddTimelessData(ReadOnlySpan<byte> data)
        {
            lock (_lock)
            {
                _data.AddTimelessData(data);
            }
        }

        public void AddTimelessData(ReadOnlySpan<byte> data, ReadOnlySpan<int> lengths)
        {
            lock (_lock)
            {
                _data.AddTimelessData(data, lengths);
            }
        }

        public void AddTimestampedData(ReadOnlySpan<byte> data, long timestamp)
        {
            lock (_lock)
            {
                _data.AddTimestampedData(data, timestamp);
            }
        }

        public void AddTimestampedData(ReadOnlySpan<byte> data, ReadOnlySpan<int> lengths, ReadOnlySpan<long> timestamps)
        {
            lock (_lock)
            {
                _data.AddTimestampedData(data, lengths, timestamps);
            }
        }

        public void Dispose()
        {
            _data.Dispose();
        }
    }
}