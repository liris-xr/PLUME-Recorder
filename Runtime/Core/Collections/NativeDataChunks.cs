using System;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace PLUME.Core.Collections
{
    [BurstCompile]
    [StructLayout(LayoutKind.Sequential)]
    [NativeContainer]
    [GenerateTestsForBurstCompatibility]
    public struct NativeDataChunks : IDisposable
    {
        private NativeList<byte> _data;
        private NativeList<int> _chunkLengths;

        public NativeDataChunks(Allocator allocator)
        {
            _data = new NativeList<byte>(allocator);
            _chunkLengths = new NativeList<int>(allocator);
        }

        public void Add(ReadOnlySpan<byte> data)
        {
            unsafe
            {
                fixed (byte* dataPtr = data)
                {
                    _data.AddRange(dataPtr, data.Length);
                }

                _chunkLengths.Add(data.Length);
            }
        }

        public void Add(ReadOnlySpan<byte> data, ReadOnlySpan<int> lengths)
        {
            unsafe
            {
                fixed (byte* dataPtr = data)
                {
                    _data.AddRange(dataPtr, data.Length);
                }

                fixed (int* lengthsPtr = lengths)
                {
                    _chunkLengths.AddRange(lengthsPtr, lengths.Length);
                }
            }
        }

        public void RemoveAt(int chunkIndex)
        {
            var firstByteIndex = GetChunkFirstByteIndex(chunkIndex);
            var chunkLength = _chunkLengths[chunkIndex];
            _data.RemoveRange(firstByteIndex, firstByteIndex + chunkLength);
            _chunkLengths.RemoveAt(chunkIndex);
        }

        public void RemoveRange(int chunkIndex, int count)
        {
            var firstByteIndex = GetChunkFirstByteIndex(chunkIndex);
            var lastByteIndex = GetChunkLastByteIndex(chunkIndex + count - 1);
            _data.RemoveRange(firstByteIndex, lastByteIndex);
            _chunkLengths.RemoveRange(chunkIndex, count);
        }

        public bool TryPop(NativeList<byte> dataDst, out int chunkLength)
        {
            dataDst.Clear();

            if (ChunkCount == 0)
            {
                chunkLength = 0;
                return false;
            }

            var lastChunkIndex = ChunkCount - 1;
            var lastChunkLength = _chunkLengths[lastChunkIndex];
            chunkLength = lastChunkLength;

            unsafe
            {
                dataDst.AddRange(GetChunksData(lastChunkIndex, 1).GetUnsafeReadOnlyPtr(), lastChunkLength);
            }

            RemoveAt(lastChunkIndex);
            return true;
        }

        public bool TryPopAll(NativeList<byte> dataDst, NativeList<int> chunkLengths)
        {
            dataDst.Clear();
            chunkLengths.Clear();

            if (ChunkCount == 0)
            {
                return false;
            }

            unsafe
            {
                dataDst.AddRange(_data.GetUnsafeReadOnlyPtr(), _data.Length);
                chunkLengths.AddRange(_chunkLengths.GetUnsafeReadOnlyPtr(), _chunkLengths.Length);
            }

            Clear();
            return true;
        }

        /// <summary>
        /// Merge the data with the chunk at the given index. The data is merged at the end of the chunk.
        /// </summary>
        /// <param name="chunkIndex"></param>
        /// <param name="data"></param>
        /// <param name="mergePos"></param>
        public void MergeIntoChunk(int chunkIndex, ReadOnlySpan<byte> data, MergePosition mergePos = MergePosition.End)
        {
            var dataIndex = mergePos == MergePosition.Start
                ? GetChunkFirstByteIndex(chunkIndex)
                : GetChunkLastByteIndex(chunkIndex);

            _data.InsertRange(dataIndex, data.Length);
            data.CopyTo(_data.AsArray().AsSpan().Slice(dataIndex, data.Length));
            _chunkLengths[chunkIndex] += data.Length;
        }

        public void InsertAfterChunk(int chunkIndex, ReadOnlySpan<byte> data)
        {
            var chunkLastByteIndex = GetChunkLastByteIndex(chunkIndex);
            _data.InsertRange(chunkLastByteIndex, data.Length);
            data.CopyTo(_data.AsArray().AsSpan().Slice(chunkLastByteIndex, data.Length));
            _chunkLengths.InsertRange(chunkIndex + 1, 1);
            _chunkLengths[chunkIndex + 1] = data.Length;
        }

        public void InsertBeforeChunk(int chunkIndex, ReadOnlySpan<byte> data)
        {
            var chunkFirstByteIndex = GetChunkFirstByteIndex(chunkIndex);
            _data.InsertRange(chunkFirstByteIndex, data.Length);
            data.CopyTo(_data.AsArray().AsSpan().Slice(chunkFirstByteIndex, data.Length));
            _chunkLengths.InsertRange(chunkIndex, 1);
            _chunkLengths[chunkIndex] = data.Length;
        }

        public int GetChunkFirstByteIndex(int chunkIdx)
        {
            // Perform a cumulative sum of the chunk lengths.
            // We either start summing from the start or the end of the data to minimize the number of iterations.
            if (chunkIdx > ChunkCount / 2)
            {
                var firstByteIndex = _data.Length;

                for (var i = ChunkCount - 1; i >= chunkIdx; i--)
                {
                    firstByteIndex -= _chunkLengths[i];
                }

                return firstByteIndex;
            }
            else
            {
                var firstByteIndex = 0;

                for (var i = 0; i < chunkIdx; i++)
                {
                    firstByteIndex += _chunkLengths[i];
                }

                return firstByteIndex;
            }
        }

        public int GetChunkLastByteIndex(int chunkIdx)
        {
            // Perform a cumulative sum of the chunk lengths.
            // We either start summing from the start or the end of the data to minimize the number of iterations.
            if (chunkIdx > ChunkCount / 2)
            {
                var lastByteIndex = _data.Length;

                for (var i = ChunkCount - 1; i > chunkIdx; i--)
                {
                    lastByteIndex -= _chunkLengths[i];
                }

                return lastByteIndex;
            }
            else
            {
                var lastByteIndex = 0;

                for (var i = 0; i <= chunkIdx; i++)
                {
                    lastByteIndex += _chunkLengths[i];
                }

                return lastByteIndex;
            }
        }

        public void Clear()
        {
            _data.Clear();
            _chunkLengths.Clear();
        }

        public void Dispose()
        {
            _data.Dispose();
            _chunkLengths.Dispose();
        }

        public NativeArray<byte>.ReadOnly GetChunksData(int chunkIndex, int count)
        {
            var firstByteIndex = GetChunkFirstByteIndex(chunkIndex);
            var lastByteIndex = GetChunkLastByteIndex(chunkIndex + count - 1);
            return _data.AsArray().GetSubArray(firstByteIndex, lastByteIndex - firstByteIndex).AsReadOnly();
        }

        public NativeArray<int>.ReadOnly GetChunksLengths(int chunkIndex, int count)
        {
            return _chunkLengths.AsArray().GetSubArray(chunkIndex, count).AsReadOnly();
        }

        public int Length => _data.Length;

        public int ChunkCount => _chunkLengths.Length;

        public NativeArray<byte>.ReadOnly RawData => _data.AsArray().AsReadOnly();

        public NativeArray<int>.ReadOnly ChunkLengths => _chunkLengths.AsArray().AsReadOnly();

        public enum MergePosition
        {
            Start,
            End
        }
    }
}