using System;
using Unity.Collections;

namespace PLUME.Core.Recorder.Data
{
    public class ConcurrentRecordData : IRecordData, IDisposable
    {
        private NativeRecorderData _data;
        private readonly object _lock = new();

        public ConcurrentRecordData(Allocator allocator)
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

        public void AddTimestampedData(ReadOnlySpan<byte> data, ReadOnlySpan<int> lengths,
            ReadOnlySpan<long> timestamps)
        {
            lock (_lock)
            {
                _data.AddTimestampedData(data, lengths, timestamps);
            }
        }

        public bool TryPopTimelessData(NativeList<byte> dataDst, NativeList<int> chunkLengthsDst)
        {
            lock (_lock)
            {
                return _data.TryPopTimelessData(dataDst, chunkLengthsDst);
            }
        }

        public bool TryPopTimestampedDataBeforeTimestamp(long timestamp, NativeList<byte> dataDst, NativeList<int> chunkLengthsDst,
            NativeList<long> timestampsDst, bool inclusive)
        {
            lock (_lock)
            {
                return _data.TryPopTimestampedDataBeforeTimestamp(timestamp, dataDst, chunkLengthsDst, timestampsDst, inclusive);
            }
        }

        public bool TryPopAllTimestampedData(NativeList<byte> timestampedData, NativeList<int> timestampedLengths, NativeList<long> timestamps)
        {
            lock (_lock)
            {
                return _data.TryPopAllTimestampedData(timestampedData, timestampedLengths, timestamps);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _data.Clear();
            }
        }

        public int GetTimelessDataLength()
        {
            lock (_lock)
            {
                return _data.GetTimelessDataLength();
            }
        }

        public int GetTimestampedDataLength()
        {
            lock (_lock)
            {
                return _data.GetTimestampedDataLength();
            }
        }

        public void Dispose()
        {
            _data.Dispose();
        }
    }
}