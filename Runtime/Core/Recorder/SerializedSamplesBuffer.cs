using System;
using Unity.Collections;

namespace PLUME.Core.Recorder
{
    // TODO: merge with NativeDataChunks/NativeTimestampedDataChunks
    public struct SerializedSamplesBuffer : IDisposable
    {
        private NativeList<byte> _data;
        private NativeList<int> _lengths;
        private NativeList<SampleTypeUrlIndex> _sampleTypeUrlIndices;
        public int ChunkCount => _lengths.Length;

        public SerializedSamplesBuffer(Allocator allocator)
        {
            _data = new NativeList<byte>(allocator);
            _lengths = new NativeList<int>(allocator);
            _sampleTypeUrlIndices = new NativeList<SampleTypeUrlIndex>(allocator);
        }

        public void Merge(SerializedSamplesBuffer other)
        {
            _data.AddRange(other._data.AsArray());
            _lengths.AddRange(other._lengths.AsArray());
            _sampleTypeUrlIndices.AddRange(other._sampleTypeUrlIndices.AsArray());
        }

        public void AddSerializedSample(SampleTypeUrlIndex sampleTypeUrlIndex, ReadOnlySpan<byte> data)
        {
            unsafe
            {
                fixed (byte* ptr = data)
                {
                    _data.AddRange(ptr, data.Length);
                }

                _lengths.Add(data.Length);
                _sampleTypeUrlIndices.Add(sampleTypeUrlIndex);
            }
        }

        public void AddSerializedSamples(SampleTypeUrlIndex sampleTypeUrlIndex, ReadOnlySpan<byte> data,
            ReadOnlySpan<int> lengths)
        {
            unsafe
            {
                fixed (byte* ptr = data)
                {
                    _data.AddRange(ptr, data.Length);
                }

                // TODO: in case a chunk misreports its length, we should check that the sum of lengths is equal to the data length
                // TODO: we should do this when packing the frame data because then we can sum the lengths as we go
                fixed (int* ptr = lengths)
                {
                    _lengths.AddRange(ptr, lengths.Length);
                }

                _sampleTypeUrlIndices.AddReplicate(sampleTypeUrlIndex, lengths.Length);
            }
        }

        public void AddSerializedSamples(ReadOnlySpan<SampleTypeUrlIndex> typeUrlIndices, ReadOnlySpan<byte> data,
            ReadOnlySpan<int> lengths)
        {
            if (typeUrlIndices.Length != lengths.Length)
                throw new ArgumentException(
                    $"{nameof(typeUrlIndices)} and {nameof(lengths)} must have the same length.");

            unsafe
            {
                fixed (byte* ptr = data)
                {
                    _data.AddRange(ptr, data.Length);
                }

                fixed (int* ptr = lengths)
                {
                    _lengths.AddRange(ptr, lengths.Length);
                }

                fixed (SampleTypeUrlIndex* ptr = typeUrlIndices)
                {
                    _sampleTypeUrlIndices.AddRange(ptr, typeUrlIndices.Length);
                }
            }
        }

        public NativeArray<byte> GetData()
        {
            return _data.AsArray();
        }

        public NativeArray<byte> GetData(int start, int length)
        {
            return _data.AsArray().GetSubArray(start, length);
        }

        public NativeArray<int> GetLengths()
        {
            return _lengths.AsArray();
        }

        public NativeArray<SampleTypeUrlIndex> GetSampleTypeUrlIndices()
        {
            return _sampleTypeUrlIndices.AsArray();
        }

        public void Dispose()
        {
            _data.Dispose();
            _lengths.Dispose();
            _sampleTypeUrlIndices.Dispose();
        }
    }
}