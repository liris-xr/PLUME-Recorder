using System;

namespace PLUME.Core.Recorder
{
    public interface IRecorderData
    {
        public void AddTimelessData(ReadOnlySpan<byte> data);

        public void AddTimelessData(ReadOnlySpan<byte> data, ReadOnlySpan<int> lengths);

        public void AddTimestampedData(ReadOnlySpan<byte> data, long timestamp);

        public void AddTimestampedData(ReadOnlySpan<byte> data, ReadOnlySpan<int> lengths,
            ReadOnlySpan<long> timestamps);
    }
}