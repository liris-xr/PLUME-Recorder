using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace PLUME.Core.Recorder.Data
{
    [BurstCompile]
    [StructLayout(LayoutKind.Sequential)]
    [NativeContainer]
    [GenerateTestsForBurstCompatibility]
    public struct NativeTimestampedDataChunks : IDisposable
    {
        private NativeDataChunks _data;
        private NativeList<long> _timestamps;

        public NativeTimestampedDataChunks(Allocator allocator)
        {
            _data = new NativeDataChunks(allocator);
            _timestamps = new NativeList<long>(allocator);
        }

        public void Enqueue(ReadOnlySpan<byte> data, ReadOnlySpan<int> chunkLengths, ReadOnlySpan<long> timestamps)
        {
            if (timestamps.Length != chunkLengths.Length)
                throw new ArgumentException($"Timestamps and chunk lengths must have the same length. " +
                                            $"Timestamps length: {timestamps.Length}, chunk lengths length: {chunkLengths.Length}");

            if (data.Length == 0)
                return;

            var nChunks = timestamps.Length;
            var offset = 0;

            for (var i = 0; i < nChunks; i++)
            {
                Enqueue(data.Slice(offset, chunkLengths[i]), timestamps[i]);
                offset += chunkLengths[i];
            }
        }

        public void Enqueue(ReadOnlySpan<byte> data, long timestamp)
        {
            if (data.Length == 0)
                return;

            // If the list is empty or the timestamp is greater than the last one, just add it to the end.
            if (ChunkCount == 0 || timestamp > _timestamps[ChunkCount - 1])
            {
                _data.Add(data);
                _timestamps.Add(timestamp);
                return;
            }

            var chunkIndex = FirstChunkIndexAfterTimestampInclusive(timestamp);

            if (_timestamps[chunkIndex] == timestamp)
            {
                _data.MergeIntoChunk(chunkIndex, data);
            }
            else // timestamp < _timestamps[idx]
            {
                _data.InsertBeforeChunk(chunkIndex, data);
                _timestamps.InsertRange(chunkIndex, 1);
                _timestamps[chunkIndex] = timestamp;
            }
        }

        // TODO: Merge TryDequeueAll implementations
        public unsafe bool TryDequeueAll(NativeList<byte> dataDst, NativeList<int> chunkLengthsDst,
            NativeList<long> timestampsDst)
        {
            dataDst.Clear();
            chunkLengthsDst.Clear();
            timestampsDst.Clear();

            if (ChunkCount == 0)
                return false;

            dataDst.AddRange(_data.RawData.GetUnsafeReadOnlyPtr(), _data.DataLength);
            chunkLengthsDst.AddRange(_data.ChunkLengths.GetUnsafeReadOnlyPtr(), _data.ChunkCount);
            timestampsDst.AddRange(_timestamps.GetUnsafeReadOnlyPtr(), _timestamps.Length);
            _data.Clear();
            _timestamps.Clear();
            return true;
        }

        // TODO: Merge TryDequeueAll implementations
        public bool TryDequeueAll(List<byte> dataDst, List<int> chunkLengthsDst, List<long> timestampsDst)
        {
            dataDst.Clear();
            chunkLengthsDst.Clear();
            timestampsDst.Clear();

            if (ChunkCount == 0)
                return false;

            dataDst.AddRange(_data.RawData);
            chunkLengthsDst.AddRange(_data.ChunkLengths);
            timestampsDst.AddRange(_timestamps);
            _data.Clear();
            _timestamps.Clear();
            return true;
        }
        
        // TODO: Merge TryDequeueAllBeforeTimestamp implementations
        public unsafe bool TryDequeueAllBeforeTimestamp(long timestamp, NativeList<byte> dataDst,
            NativeList<int> chunkLengthsDst, NativeList<long> timestampsDst, bool inclusive)
        {
            dataDst.Clear();
            chunkLengthsDst.Clear();
            timestampsDst.Clear();

            if (ChunkCount == 0)
                return false;

            if (timestamp < _timestamps[0])
                return false;

            // If the timestamp is greater than the last one, just return all the data.
            if (timestamp > _timestamps[ChunkCount - 1])
            {
                dataDst.AddRange(_data.RawData.GetUnsafeReadOnlyPtr(), _data.DataLength);
                chunkLengthsDst.AddRange(_data.ChunkLengths.GetUnsafeReadOnlyPtr(), _data.ChunkCount);
                timestampsDst.AddRange(_timestamps.GetUnsafeReadOnlyPtr(), _timestamps.Length);
                _data.Clear();
                _timestamps.Clear();
                return true;
            }

            var chunkIndex = FirstChunkIndexAfterTimestampInclusive(timestamp);
            
            if (_timestamps[chunkIndex] > timestamp || (_timestamps[chunkIndex] == timestamp && !inclusive))
                chunkIndex -= 1;
            
            // Nothing to copy.
            if (chunkIndex < 0)
                return false;

            // We dequeue all chunks between index 0 and lastChunkIndex.
            var nChunks = chunkIndex + 1;

            var rawData = _data.GetChunksData(0, nChunks);
            var chunkLengths = _data.GetChunksLengths(0, nChunks);
            dataDst.AddRange(rawData.GetUnsafeReadOnlyPtr(), rawData.Length);
            chunkLengthsDst.AddRange(chunkLengths.GetUnsafeReadOnlyPtr(), chunkLengths.Length);
            timestampsDst.AddRange(_timestamps.GetUnsafeReadOnlyPtr(), nChunks);
            _data.RemoveRange(0, nChunks);
            _timestamps.RemoveRange(0, nChunks);
            return true;
        }
        
        // TODO: Merge TryDequeueAllBeforeTimestamp implementations
        public bool TryDequeueAllBeforeTimestamp(long timestamp, List<byte> dataDst,
            List<int> chunkLengthsDst, List<long> timestampsDst, bool inclusive)
        {
            dataDst.Clear();
            chunkLengthsDst.Clear();
            timestampsDst.Clear();

            if (ChunkCount == 0)
                return false;

            if (timestamp < _timestamps[0])
                return false;

            // If the timestamp is greater than the last one, just return all the data.
            if (timestamp > _timestamps[ChunkCount - 1])
            {
                dataDst.AddRange(_data.RawData);
                chunkLengthsDst.AddRange(_data.ChunkLengths);
                timestampsDst.AddRange(_timestamps);
                _data.Clear();
                _timestamps.Clear();
                return true;
            }

            var chunkIndex = FirstChunkIndexAfterTimestampInclusive(timestamp);
            
            if (_timestamps[chunkIndex] > timestamp || (_timestamps[chunkIndex] == timestamp && !inclusive))
                chunkIndex -= 1;
            
            // Nothing to copy.
            if (chunkIndex < 0)
                return false;

            // We dequeue all chunks between index 0 and lastChunkIndex.
            var nChunks = chunkIndex + 1;

            var rawData = _data.GetChunksData(0, nChunks);
            var chunkLengths = _data.GetChunksLengths(0, nChunks);
            dataDst.AddRange(rawData);
            chunkLengthsDst.AddRange(chunkLengths);
            timestampsDst.AddRange(_timestamps);
            _data.RemoveRange(0, nChunks);
            _timestamps.RemoveRange(0, nChunks);
            return true;
        }
        
        private int FirstChunkIndexAfterTimestampInclusive(long timestamp)
        {
            var left = 0;
            var right = ChunkCount - 1;
            while (left <= right)
            {
                var middle = left + (right - left) / 2;
                if (_timestamps[middle] == timestamp)
                    return middle;
                if (_timestamps[middle] < timestamp)
                    left = middle + 1;
                else
                    right = middle - 1;
            }

            return left;
        }

        public int DataLength => _data.DataLength;

        public int ChunkCount => _data.ChunkCount;

        public NativeArray<byte>.ReadOnly RawData => _data.RawData;

        public NativeArray<int>.ReadOnly ChunkLengths => _data.ChunkLengths;

        public void Clear()
        {
            _data.Clear();
            _timestamps.Clear();
        }

        public void Dispose()
        {
            _data.Dispose();
            _timestamps.Dispose();
        }

        public NativeArray<byte>.ReadOnly GetRawData()
        {
            return _data.RawData;
        }

        public NativeArray<long>.ReadOnly ChunkTimestamps => _timestamps.AsReadOnly();
    }
}