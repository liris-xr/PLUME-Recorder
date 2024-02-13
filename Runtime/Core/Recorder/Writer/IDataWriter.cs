using System;

namespace PLUME.Core.Recorder.Writer
{
    public interface IDataWriter
    {
        public void WriteTimelessData(ReadOnlySpan<byte> data, ReadOnlySpan<int> lengths);

        public void WriteTimestampedData(ReadOnlySpan<byte> data, ReadOnlySpan<int> lengths,
            ReadOnlySpan<long> timestamps);

        public void Close();
    }
}