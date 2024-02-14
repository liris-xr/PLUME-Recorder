using System;
using Unity.Collections;

namespace PLUME.Core.Recorder.Data
{
    public interface IRecordData : IDisposable
    {
        public void AddTimelessData(ReadOnlySpan<byte> data);

        public void AddTimelessData(ReadOnlySpan<byte> data, ReadOnlySpan<int> lengths);

        public void AddTimestampedData(ReadOnlySpan<byte> data, long timestamp);

        public void AddTimestampedData(ReadOnlySpan<byte> data, ReadOnlySpan<int> lengths, ReadOnlySpan<long> timestamps);

        public bool TryPopTimelessData(NativeList<byte> dataDst, NativeList<int> chunkLengthsDst);

        public bool TryPopTimestampedDataBeforeTimestamp(long timestamp, NativeList<byte> dataDst,
            NativeList<int> chunkLengthsDst, NativeList<long> timestampsDst, bool inclusive);

        bool TryPopAllTimestampedData(NativeList<byte> timestampedData, NativeList<int> timestampedLengths, NativeList<long> timestamps);
        
        public void Clear();

        public int GetTimelessDataLength();

        public int GetTimestampedDataLength();
    }
}