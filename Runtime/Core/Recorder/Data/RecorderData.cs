using System;
using Unity.Collections;

namespace PLUME.Core.Recorder.Data
{
    public struct NativeRecorderData : IDisposable
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

        public void AddTimestampedData(ReadOnlySpan<byte> data, ReadOnlySpan<int> lengths,
            ReadOnlySpan<long> timestamps)
        {
            _timestampedData.Enqueue(data, lengths, timestamps);
        }

        public bool TryPopTimelessData(NativeList<byte> dataDst, NativeList<int> chunkLengthsDst)
        {
            return _timelessData.TryPopAll(dataDst, chunkLengthsDst);
        }

        public bool TryPopTimestampedDataBeforeTimestamp(long timestamp, NativeList<byte> dataDst,
            NativeList<int> chunkLengthsDst, NativeList<long> timestampsDst, bool inclusive)
        {
            return _timestampedData.TryDequeueAllBeforeTimestamp(timestamp, dataDst, chunkLengthsDst, timestampsDst,
                inclusive);
        }
        
        public bool TryPopAllTimestampedData(NativeList<byte> timestampedData, NativeList<int> timestampedLengths, NativeList<long> timestamps)
        {
            return _timestampedData.TryDequeueAll(timestampedData, timestampedLengths, timestamps);
        }

        public void Clear()
        {
            _timelessData.Clear();
            _timestampedData.Clear();
        }

        public int GetTimelessDataLength()
        {
            return _timelessData.DataLength;
        }

        public int GetTimestampedDataLength()
        {
            return _timestampedData.DataLength;
        }

        public void Dispose()
        {
            _timelessData.Dispose();
            _timestampedData.Dispose();
        }
    }
}