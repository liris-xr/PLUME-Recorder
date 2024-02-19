using System;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace PLUME.Core.Recorder.Data
{
    [BurstCompile]
    [GenerateTestsForBurstCompatibility]
    public struct DataChunks : IReadOnlyDataChunks, IDisposable
    {
        private NativeList<byte> _data;
        private NativeList<int> _chunksLengths;

        public int ChunksCount => _chunksLengths.Length;
        public int DataLength => _data.Length;

        public DataChunks(Allocator allocator)
        {
            _data = new NativeList<byte>(allocator);
            _chunksLengths = new NativeList<int>(allocator);
        }

        public DataChunks(int dataCapacity, int chunksCapacity, Allocator allocator)
        {
            _data = new NativeList<byte>(dataCapacity, allocator);
            _chunksLengths = new NativeList<int>(chunksCapacity, allocator);
        }

        public DataChunks(ReadOnlySpan<byte> chunksData, ReadOnlySpan<int> chunksLength, Allocator allocator)
        {
            _data = new NativeList<byte>(chunksData.Length, allocator);
            _chunksLengths = new NativeList<int>(chunksLength.Length, allocator);
            AddRange(chunksData, chunksLength);
        }

        public DataChunks(DataChunks other, Allocator allocator)
        {
            _data = new NativeList<byte>(other._data.Length, allocator);
            _chunksLengths = new NativeList<int>(other._chunksLengths.Length, allocator);
            AddRange(other._data.AsArray(), other._chunksLengths.AsArray());
        }

        public void Add(DataChunks chunks)
        {
            CheckIsCreated();
            InsertRange(ChunksCount, chunks._data.AsArray(), chunks._chunksLengths.AsArray());
        }

        public void AddReplicate(ReadOnlySpan<byte> chunk, int count)
        {
            CheckIsCreated();
            var dataIndex = _data.Length;
            
            _data.ResizeUninitialized(_data.Length + chunk.Length * count);

            unsafe
            {
                fixed (byte* chunkPtr = chunk)
                {
                    UnsafeUtility.MemCpyReplicate(_data.GetUnsafePtr() + dataIndex, chunkPtr, chunk.Length, count);
                }
            }
            
            _chunksLengths.AddReplicate(chunk.Length, count);
        }

        public void Add(ReadOnlySpan<byte> chunk)
        {
            CheckIsCreated();
            Insert(ChunksCount, chunk);
        }

        public void AddRange(ReadOnlySpan<byte> chunksData, ReadOnlySpan<int> chunksLength)
        {
            CheckIsCreated();
            InsertRange(ChunksCount, chunksData, chunksLength);
        }

        public void AddUninitialized(int chunkLength)
        {
            CheckIsCreated();
            _data.ResizeUninitialized(_data.Length + chunkLength);
            _chunksLengths.Add(chunkLength);
        }

        public void Insert(int chunkIndex, DataChunks chunks)
        {
            CheckIsCreated();
            InsertRange(chunkIndex, chunks._data.AsArray(), chunks._chunksLengths.AsArray());
        }

        public void Insert(int chunkIndex, ReadOnlySpan<byte> chunkData)
        {
            CheckIsCreated();
            if (chunkData.Length == 0)
                return;

            var byteIndex = GetFirstByteIndex(chunkIndex);

            _data.InsertRange(byteIndex, chunkData.Length);
            _chunksLengths.InsertRange(chunkIndex, 1);
            chunkData.CopyTo(_data.AsArray().AsSpan().Slice(byteIndex, chunkData.Length));
            _chunksLengths[chunkIndex] = chunkData.Length;
        }

        public void InsertRange(int chunkIndex, ReadOnlySpan<byte> chunkData, ReadOnlySpan<int> chunksLength)
        {
            CheckIsCreated();

            if (chunkData.Length == 0)
                return;
            if (chunksLength.Length == 0)
                return;

            var byteIndex = GetFirstByteIndex(chunkIndex);
            var chunksCount = chunksLength.Length;

            _data.InsertRange(byteIndex, chunkData.Length);
            _chunksLengths.InsertRange(chunkIndex, chunksCount);
            chunkData.CopyTo(_data.AsArray().AsSpan().Slice(byteIndex, chunkData.Length));
            chunksLength.CopyTo(_chunksLengths.AsArray().AsSpan().Slice(chunkIndex, chunksCount));
        }
        
        /// <summary>
        /// Merge the data with the chunk at the given index. The data is merged at the end of the chunk.
        /// </summary>
        /// <param name="chunkIndex"></param>
        /// <param name="chunkData"></param>
        /// <param name="pos"></param>
        public void MergeIntoChunk(int chunkIndex, ReadOnlySpan<byte> chunkData, ChunkPosition pos)
        {
            CheckIsCreated();
            if (chunkData.Length == 0)
                return;

            var byteIndex = pos == ChunkPosition.Start
                ? GetFirstByteIndex(chunkIndex)
                : GetFirstByteIndex(chunkIndex) + GetLength(chunkIndex);

            _data.InsertRange(byteIndex, chunkData.Length);
            chunkData.CopyTo(_data.AsArray().AsSpan().Slice(byteIndex, chunkData.Length));
            _chunksLengths[chunkIndex] += chunkData.Length;
        }

        public bool TryRemove(int chunkIndex, DataChunks dst)
        {
            CheckIsCreated();
            dst.Clear();

            if (ChunksCount == 0)
                return false;

            dst.Add(GetDataSpan(chunkIndex));
            Remove(chunkIndex);
            return true;
        }

        public bool TryRemoveRange(int chunkIndex, int count, DataChunks dst)
        {
            CheckIsCreated();
            dst.Clear();

            if (ChunksCount == 0)
                return false;

            dst.AddRange(GetDataSpan(chunkIndex, count), GetLengthsSpan(chunkIndex, count));
            RemoveRange(chunkIndex, count);
            return true;
        }

        public bool TryRemoveAll(DataChunks dst)
        {
            CheckIsCreated();
            dst.Clear();

            if (ChunksCount == 0)
                return false;

            dst.AddRange(GetDataSpan(), GetLengthsSpan());
            RemoveAll();
            return true;
        }

        public void Remove(int chunkIndex)
        {
            CheckIsCreated();
            var byteIndex = GetFirstByteIndex(chunkIndex);
            var chunkLength = _chunksLengths[chunkIndex];
            _data.RemoveRange(byteIndex, chunkLength);
            _chunksLengths.RemoveAt(chunkIndex);
        }

        public void RemoveRange(int chunkIndex, int count)
        {
            CheckIsCreated();
            var lastChunkIndex = chunkIndex + count - 1;
            var firstByteIndex = GetFirstByteIndex(chunkIndex);
            var lastByteIndex = GetFirstByteIndex(lastChunkIndex) + GetLength(lastChunkIndex);
            var byteLength = lastByteIndex - firstByteIndex;

            _data.RemoveRange(firstByteIndex, byteLength);
            _chunksLengths.RemoveRange(chunkIndex, count);
        }

        public void RemoveAll()
        {
            CheckIsCreated();
            _data.Clear();
            _chunksLengths.Clear();
        }

        public int GetFirstByteIndex(int chunkIdx)
        {
            CheckIsCreated();
            // Perform a cumulative sum of the chunk lengths.
            // We either start summing from the start or the end of the data to minimize the number of iterations.
            if (chunkIdx > ChunksCount / 2)
            {
                var byteIndex = DataLength;

                for (var i = ChunksCount - 1; i >= chunkIdx; i--)
                {
                    byteIndex -= _chunksLengths[i];
                }

                return byteIndex;
            }
            else
            {
                var byteIndex = 0;

                for (var i = 0; i < chunkIdx; i++)
                {
                    byteIndex += _chunksLengths[i];
                }

                return byteIndex;
            }
        }

        public bool IsEmpty()
        {
            CheckIsCreated();
            return _data.Length == 0;
        }

        public void Clear()
        {
            CheckIsCreated();
            _data.Clear();
            _chunksLengths.Clear();
        }

        public DataChunks Copy(Allocator allocator)
        {
            CheckIsCreated();
            return new DataChunks(this, allocator);
        }

        public NativeArray<byte> GetData(int chunkIndex, Allocator allocator)
        {
            CheckIsCreated();
            var chunkByteIndex = GetFirstByteIndex(chunkIndex);
            var chunkLength = _chunksLengths[chunkIndex];
            var chunkData = new NativeArray<byte>(chunkLength, allocator);
            _data.AsArray().GetSubArray(chunkByteIndex, chunkLength).CopyTo(chunkData);
            return chunkData;
        }

        public NativeArray<byte> GetData(int chunkIndex, int count, Allocator allocator)
        {
            CheckIsCreated();
            var lastChunkIndex = chunkIndex + count - 1;
            var firstByteIndex = GetFirstByteIndex(chunkIndex);
            var lastByteIndex = GetFirstByteIndex(lastChunkIndex) + GetLength(lastChunkIndex);
            var byteLength = lastByteIndex - firstByteIndex;
            var chunksData = new NativeArray<byte>(byteLength, allocator);
            _data.AsArray().GetSubArray(firstByteIndex, byteLength).CopyTo(chunksData);
            return chunksData;
        }

        public Span<byte> GetDataSpan(int chunkIndex)
        {
            CheckIsCreated();
            var chunkByteIndex = GetFirstByteIndex(chunkIndex);
            var chunkLength = _chunksLengths[chunkIndex];
            return _data.AsArray().GetSubArray(chunkByteIndex, chunkLength).AsSpan();
        }

        public Span<byte> GetDataSpan(int chunkIndex, int count)
        {
            CheckIsCreated();
            var lastChunkIndex = chunkIndex + count - 1;
            var firstByteIndex = GetFirstByteIndex(chunkIndex);
            var lastByteIndex = GetFirstByteIndex(lastChunkIndex) + GetLength(lastChunkIndex);
            var byteLength = lastByteIndex - firstByteIndex;
            if (byteLength < 0)
                Debug.LogWarning("buffer overflow?");
            return _data.AsArray().GetSubArray(firstByteIndex, byteLength).AsSpan();
        }

        public Span<byte> GetDataSpan()
        {
            CheckIsCreated();
            return _data.AsArray().AsSpan();
        }

        public NativeArray<byte> GetData(Allocator allocator)
        {
            CheckIsCreated();
            return _data.ToArray(allocator);
        }

        public int GetLength(int chunkIdx)
        {
            CheckIsCreated();
            return _chunksLengths[chunkIdx];
        }

        public Span<int> GetLengthsSpan(int chunkIndex, int count)
        {
            CheckIsCreated();
            return _chunksLengths.AsArray().GetSubArray(chunkIndex, count).AsSpan();
        }

        public NativeArray<int> GetLengths(int chunkIndex, int count, Allocator allocator)
        {
            CheckIsCreated();
            var chunksLength = new NativeArray<int>(count, allocator);
            _chunksLengths.AsArray().GetSubArray(chunkIndex, count).CopyTo(chunksLength);
            return chunksLength;
        }

        public Span<int> GetLengthsSpan()
        {
            CheckIsCreated();
            return _chunksLengths.AsArray().AsSpan();
        }

        public NativeArray<int> GetLengths(Allocator allocator)
        {
            CheckIsCreated();
            return _chunksLengths.ToArray(allocator);
        }

        public enum ChunkPosition
        {
            Start,
            End
        }

        public override int GetHashCode()
        {
            var dataHash = _data.Length;
            var lengthHash = _chunksLengths.Length;

            // ReSharper disable once ForCanBeConvertedToForeach
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (var i = 0; i < _data.Length; i++)
            {
                dataHash = unchecked(dataHash * 31 + _data[i]);
            }

            // ReSharper disable once ForCanBeConvertedToForeach
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (var i = 0; i < _chunksLengths.Length; i++)
            {
                lengthHash = unchecked(lengthHash * 31 + _chunksLengths[i]);
            }

            return HashCode.Combine(dataHash, lengthHash);
        }

        public override bool Equals(object obj)
        {
            if (!IsCreated)
                return false;
            if (obj is not DataChunks other)
                return false;
            if (!other.IsCreated)
                return false;
            if (ChunksCount != other.ChunksCount) return false;
            if (DataLength != other.DataLength) return false;

            for (var i = 0; i < ChunksCount; i++)
            {
                if (_chunksLengths[i] != other._chunksLengths[i])
                    return false;
            }

            for (var i = 0; i < DataLength; i++)
            {
                if (_data[i] != other._data[i])
                    return false;
            }

            return true;
        }

        public void Dispose()
        {
            if (!IsCreated)
                throw new InvalidOperationException($"{nameof(DataChunks)} is not created.");

            _data.Dispose();
            _chunksLengths.Dispose();
        }

        public override string ToString()
        {
            CheckIsCreated();

            var sb = new StringBuilder();

            sb.Append($"{ChunksCount} chunks, {_data.Length} bytes (");

            for (var i = 0; i < ChunksCount; i++)
            {
                sb.Append($"{_chunksLengths[i]}");

                if (i < ChunksCount - 1)
                {
                    sb.Append(", ");
                }
            }

            sb.Append(")");

            return sb.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckIsCreated()
        {
            if (!IsCreated)
                throw new InvalidOperationException($"{nameof(DataChunks)} is not created.");
        }

        public bool IsCreated => _data.IsCreated && _chunksLengths.IsCreated;
    }
}