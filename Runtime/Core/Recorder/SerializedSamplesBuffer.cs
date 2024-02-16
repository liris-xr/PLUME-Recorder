using System;
using ProtoBurst;
using Unity.Collections;

namespace PLUME.Core.Recorder
{
    // TODO: merge with RecordData 
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

        public void EnsureCapacity(int requiredBytesCapacity, int requiredChunksCapacity)
        {
            if (requiredBytesCapacity > _data.Capacity)
            {
                _data.SetCapacity(requiredBytesCapacity);
            }

            if (requiredChunksCapacity > _lengths.Capacity)
            {
                _lengths.SetCapacity(requiredChunksCapacity);
            }

            if (requiredChunksCapacity > _sampleTypeUrlIndices.Capacity)
            {
                _sampleTypeUrlIndices.SetCapacity(requiredChunksCapacity);
            }
        }

        public void AddSerializedSample(SampleTypeUrlIndex sampleTypeUrlIndex, ReadOnlySpan<byte> data)
        {
            unsafe
            {
                fixed (byte* ptr = data)
                {
                    _data.AddRange(ptr, data.Length);
                }
            }

            _lengths.Add(data.Length);
            _sampleTypeUrlIndices.Add(sampleTypeUrlIndex);
        }

        public void AddSerializedSample(SampleTypeUrlIndex sampleTypeUrlIndex, NativeArray<byte> data)
        {
            _data.AddRange(data);
            _lengths.Add(data.Length);
            _sampleTypeUrlIndices.Add(sampleTypeUrlIndex);
        }

        public void AddSerializedSamples(SampleTypeUrlIndex sampleTypeUrlIndex, NativeArray<byte> data,
            NativeArray<int> lengths)
        {
            _data.AddRange(data);
            _lengths.AddRange(lengths);
            _sampleTypeUrlIndices.AddReplicate(sampleTypeUrlIndex, lengths.Length);
        }

        public void AddSerializedSampleNoResize(SampleTypeUrlIndex sampleTypeUrlIndex, NativeArray<byte> data)
        {
            _data.AddRangeNoResize(data);
            _lengths.AddNoResize(data.Length);
            _sampleTypeUrlIndices.AddNoResize(sampleTypeUrlIndex);
        }

        public void AddSerializedSamplesNoResize(SampleTypeUrlIndex sampleTypeUrlIndex, NativeArray<byte> data,
            NativeArray<int> lengths)
        {
            _data.AddRangeNoResize(data);
            _lengths.AddRangeNoResize(lengths);
            _sampleTypeUrlIndices.AddReplicateNoResize(sampleTypeUrlIndex, lengths.Length);
        }

        public void AddSerializedSamples(NativeArray<SampleTypeUrlIndex> typeUrlIndices, NativeArray<byte> data,
            NativeArray<int> lengths)
        {
            if (typeUrlIndices.Length != lengths.Length)
                throw new ArgumentException(
                    $"{nameof(typeUrlIndices)} and {nameof(lengths)} must have the same length.");

            _data.AddRange(data);
            _lengths.AddRange(lengths);
            _sampleTypeUrlIndices.AddRange(typeUrlIndices);
        }

        public int GetDataCapacity()
        {
            return _data.Capacity;
        }

        public int GetChunksCapacity()
        {
            return ChunkCount;
        }

        public NativeArray<byte> GetData()
        {
            return _data.AsArray();
        }
        
        public NativeArray<byte> GetData(Allocator allocator)
        {
            var copy = new NativeArray<byte>(_data.Length, allocator);
            _data.AsArray().CopyTo(copy);
            return copy;
        }

        public NativeArray<byte> GetData(int start, int length)
        {
            return _data.AsArray().GetSubArray(start, length);
        }

        public NativeArray<byte> GetData(Allocator allocator, int start, int length)
        {
            var copy = new NativeArray<byte>(length, allocator);
            _data.AsArray().GetSubArray(start, length).CopyTo(copy);
            return copy;
        }

        public int GetLength(int index)
        {
            return _lengths[index];
        }
        
        public NativeArray<int> GetLengths()
        {
            return _lengths.AsArray();
        }

        public SampleTypeUrlIndex GetSampleTypeUrlIndex(int index)
        {
            return _sampleTypeUrlIndices[index];
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