using System;

namespace PLUME.Core.Writer
{
    public interface IDataWriter
    {
        public void WriteTimelessData(ReadOnlySpan<byte> data);
        
        public void WriteTimelessData(ReadOnlySpan<byte> data, ReadOnlySpan<int> lengths);

        public void WriteTimestampedData(ReadOnlySpan<byte> data, long timestamp);

        public void WriteTimestampedData(ReadOnlySpan<byte> data, ReadOnlySpan<int> lengths, ReadOnlySpan<long> timestamps);
        
        public void Close();
    }
}