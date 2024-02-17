using System;
using System.Collections.Generic;
using Google.Protobuf;
using PLUME.Core.Utils;
using ProtoBurst;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Core.Recorder.Data
{
    [BurstCompile]
    public struct FrameDataChunks : IReadOnlyDataChunks, IDisposable
    {
        private DataChunks _dataChunks;
        private NativeList<SampleTypeUrlIndex> _sampleTypeUrlIndices;

        public int ChunksCount => _dataChunks.ChunksCount;

        public int ChunksTotalLength => _dataChunks.ChunksTotalLength;

        public FrameDataChunks(Allocator allocator)
        {
            _dataChunks = new DataChunks(allocator);
            _sampleTypeUrlIndices = new NativeList<SampleTypeUrlIndex>(allocator);
        }

        internal void Add(FrameDataChunks other)
        {
            _dataChunks.Add(other._dataChunks);
            _sampleTypeUrlIndices.AddRange(other._sampleTypeUrlIndices);
        }

        [BurstDiscard]
        public void AddSamples(IEnumerable<IMessage> samples, ref SampleTypeUrlRegistry sampleTypeUrlRegistry)
        {
            foreach (var message in samples)
            {
                AddSample(message, ref sampleTypeUrlRegistry);
            }
        }

        [BurstDiscard]
        public void AddSample(IMessage sample, ref SampleTypeUrlRegistry sampleTypeUrlRegistry)
        {
            var bytes = sample.Serialize(Allocator.Persistent);
            _dataChunks.Add(bytes);
            _sampleTypeUrlIndices.Add(
                sampleTypeUrlRegistry.GetOrCreateTypeUrlIndex("type.googleapis.com/" + sample.Descriptor.FullName));
            bytes.Dispose();
        }

        public void AddSamples<T>(NativeList<T> samples, ref SampleTypeUrlRegistry sampleTypeUrlRegistry)
            where T : unmanaged, IProtoBurstMessage
        {
            AddSamples(samples.AsArray(), ref sampleTypeUrlRegistry);
        }

        public void AddSamples<T>(NativeArray<T> samples, ref SampleTypeUrlRegistry sampleTypeUrlRegistry)
            where T : unmanaged, IProtoBurstMessage
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < samples.Length; i++)
            {
                var sample = samples[i];
                AddSample(sample, ref sampleTypeUrlRegistry);
            }
        }

        public void AddSamples<T>(NativeList<T> samples, SampleTypeUrlIndex sampleTypeUrlIndex)
            where T : unmanaged, IProtoBurstMessage
        {
            AddSamples(samples.AsArray(), sampleTypeUrlIndex);
        }

        public void AddSamples<T>(NativeArray<T> samples, SampleTypeUrlIndex sampleTypeUrlIndex)
            where T : unmanaged, IProtoBurstMessage
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < samples.Length; i++)
            {
                var sample = samples[i];
                AddSample(sample, sampleTypeUrlIndex);
            }
        }

        public void AddSample<T>(T sample, SampleTypeUrlIndex sampleTypeUrlIndex) where T : unmanaged, IProtoBurstMessage
        {
            var bytes = sample.Serialize(Allocator.Persistent);
            _dataChunks.Add(bytes);
            _sampleTypeUrlIndices.Add(sampleTypeUrlIndex);
            bytes.Dispose();
        }

        public void AddSample<T>(T sample, ref SampleTypeUrlRegistry sampleTypeUrlRegistry)
            where T : unmanaged, IProtoBurstMessage
        {
            var bytes = sample.Serialize(Allocator.Persistent);
            _dataChunks.Add(bytes);
            _sampleTypeUrlIndices.Add(sampleTypeUrlRegistry.GetOrCreateTypeUrlIndex(sample.TypeUrl));
            bytes.Dispose();
        }

        public void AddSamples(DataChunks samplesData, SampleTypeUrlIndex sampleTypeUrlIndex)
        {
            for (var i = 0; i < samplesData.ChunksCount; i++)
            {
                var chunkData = samplesData.GetChunkData(i);
                AddSample(chunkData, sampleTypeUrlIndex);
            }
        }

        public void AddSample(ReadOnlySpan<byte> sampleData, SampleTypeUrlIndex sampleTypeUrlIndex)
        {
            _dataChunks.Add(sampleData);
            _sampleTypeUrlIndices.Add(sampleTypeUrlIndex);
        }

        public void Dispose()
        {
            _dataChunks.Dispose();
            _sampleTypeUrlIndices.Dispose();
        }

        public SampleTypeUrlIndex GetSampleTypeUrlIndex(int chunkIndex)
        {
            return _sampleTypeUrlIndices[chunkIndex];
        }

        public ReadOnlySpan<byte> GetChunkData(int chunkIndex)
        {
            return _dataChunks.GetChunkData(chunkIndex);
        }

        public ReadOnlySpan<byte> GetChunksData(int chunkIndex, int count)
        {
            return _dataChunks.GetChunksData(chunkIndex, count);
        }

        public ReadOnlySpan<byte> GetChunksData()
        {
            return _dataChunks.GetChunksData();
        }

        public int GetChunkLength(int chunkIdx)
        {
            return _dataChunks.GetChunkLength(chunkIdx);
        }

        public ReadOnlySpan<int> GetChunksLength(int chunkIndex, int count)
        {
            return _dataChunks.GetChunksLength(chunkIndex, count);
        }

        public ReadOnlySpan<int> GetChunksLength()
        {
            return _dataChunks.GetChunksLength();
        }

        public NativeArray<byte> GetChunkData(int chunkIndex, Allocator allocator)
        {
            return _dataChunks.GetChunkData(chunkIndex, allocator);
        }

        public NativeArray<byte> GetChunksData(int chunkIndex, int count, Allocator allocator)
        {
            return _dataChunks.GetChunksData(chunkIndex, count, allocator);
        }

        public NativeArray<byte> GetChunksData(Allocator allocator)
        {
            return _dataChunks.GetChunksData(allocator);
        }

        public NativeArray<int> GetChunksLength(int chunkIndex, int count, Allocator allocator)
        {
            return _dataChunks.GetChunksLength(chunkIndex, count, allocator);
        }

        public NativeArray<int> GetChunksLength(Allocator allocator)
        {
            return _dataChunks.GetChunksLength(allocator);
        }

        public static implicit operator ReadOnlySpan<byte>(FrameDataChunks frameDataChunks)
        {
            return frameDataChunks.GetChunksData();
        }
    }
}