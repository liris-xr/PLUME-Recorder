using System;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Core.Recorder.Data
{
    [BurstCompile]
    [GenerateTestsForBurstCompatibility]
    public struct DataChunks : IReadOnlyDataChunks, IDisposable
    {
        private NativeList<byte> _data;
        private NativeList<int> _chunksLength;

        public int ChunksCount => _chunksLength.Length;
        public int ChunksTotalLength => _data.Length;

        public DataChunks(Allocator allocator)
        {
            _data = new NativeList<byte>(allocator);
            _chunksLength = new NativeList<int>(allocator);
        }

        public DataChunks(ReadOnlySpan<byte> chunksData, ReadOnlySpan<int> chunksLength, Allocator allocator)
        {
            _data = new NativeList<byte>(chunksData.Length, allocator);
            _chunksLength = new NativeList<int>(chunksLength.Length, allocator);
            AddRange(chunksData, chunksLength);
        }

        public DataChunks(DataChunks other, Allocator allocator)
        {
            _data = new NativeList<byte>(other._data.Length, allocator);
            _chunksLength = new NativeList<int>(other._chunksLength.Length, allocator);
            AddRange(other._data.AsArray(), other._chunksLength.AsArray());
        }

        public void Add(DataChunks chunks)
        {
            CheckIsCreated();
            InsertRange(ChunksCount, chunks._data.AsArray(), chunks._chunksLength.AsArray());
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

        public void Insert(int chunkIndex, DataChunks chunks)
        {
            CheckIsCreated();
            InsertRange(chunkIndex, chunks._data.AsArray(), chunks._chunksLength.AsArray());
        }

        public void Insert(int chunkIndex, ReadOnlySpan<byte> chunkData)
        {
            CheckIsCreated();
            if (chunkData.Length == 0)
                return;

            var byteIndex = GetChunkByteIndex(chunkIndex);

            _data.InsertRange(byteIndex, chunkData.Length);
            _chunksLength.InsertRange(chunkIndex, 1);

            chunkData.CopyTo(_data.AsArray().AsSpan().Slice(byteIndex, chunkData.Length));
            _chunksLength[chunkIndex] = chunkData.Length;
        }

        public void InsertRange(int chunkIndex, ReadOnlySpan<byte> chunkData, ReadOnlySpan<int> chunksLength)
        {
            CheckIsCreated();
            if (chunkData.Length == 0)
                return;

            var byteIndex = GetChunkByteIndex(chunkIndex);
            var chunksCount = chunksLength.Length;

            _data.InsertRange(byteIndex, chunkData.Length);
            _chunksLength.InsertRange(chunkIndex, chunksCount);

            chunkData.CopyTo(_data.AsArray().AsSpan().Slice(byteIndex, chunkData.Length));
            chunksLength.CopyTo(_chunksLength.AsArray().AsSpan().Slice(chunkIndex, chunksCount));
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
                ? GetChunkByteIndex(chunkIndex)
                : GetChunkByteIndex(chunkIndex) + GetChunkLength(chunkIndex);

            _data.InsertRange(byteIndex, chunkData.Length);
            chunkData.CopyTo(_data.AsArray().AsSpan().Slice(byteIndex, chunkData.Length));
            _chunksLength[chunkIndex] += chunkData.Length;
        }

        public bool TryRemove(int chunkIndex, DataChunks dst)
        {
            CheckIsCreated();
            dst.Clear();

            if (ChunksCount == 0)
                return false;

            dst.Add(GetChunkData(chunkIndex));
            Remove(chunkIndex);
            return true;
        }

        public bool TryRemoveRange(int chunkIndex, int count, DataChunks dst)
        {
            CheckIsCreated();
            dst.Clear();

            if (ChunksCount == 0)
                return false;

            dst.AddRange(GetChunksData(chunkIndex, count), GetChunksLength(chunkIndex, count));
            RemoveRange(chunkIndex, count);
            return true;
        }

        public bool TryRemoveAll(DataChunks dst)
        {
            CheckIsCreated();
            dst.Clear();

            if (ChunksCount == 0)
                return false;

            var chunksCount = ChunksCount;
            dst.AddRange(GetChunksData(), GetChunksLength());
            RemoveAll();
            return true;
        }

        public void Remove(int chunkIndex)
        {
            CheckIsCreated();
            var byteIndex = GetChunkByteIndex(chunkIndex);
            var chunkLength = _chunksLength[chunkIndex];
            _data.RemoveRange(byteIndex, chunkLength);
            _chunksLength.RemoveAt(chunkIndex);
        }

        public void RemoveRange(int chunkIndex, int count)
        {
            CheckIsCreated();
            var lastChunkIndex = chunkIndex + count - 1;
            var firstByteIndex = GetChunkByteIndex(chunkIndex);
            var lastByteIndex = GetChunkByteIndex(lastChunkIndex) + GetChunkLength(lastChunkIndex);
            var byteLength = lastByteIndex - firstByteIndex;

            _data.RemoveRange(firstByteIndex, byteLength);
            _chunksLength.RemoveRange(chunkIndex, count);
        }

        public void RemoveAll()
        {
            CheckIsCreated();
            _data.Clear();
            _chunksLength.Clear();
        }

        public int GetChunkByteIndex(int chunkIdx)
        {
            CheckIsCreated();
            // Perform a cumulative sum of the chunk lengths.
            // We either start summing from the start or the end of the data to minimize the number of iterations.
            if (chunkIdx > ChunksCount / 2)
            {
                var byteIndex = ChunksTotalLength;

                for (var i = ChunksCount - 1; i >= chunkIdx; i--)
                {
                    byteIndex -= _chunksLength[i];
                }

                return byteIndex;
            }
            else
            {
                var byteIndex = 0;

                for (var i = 0; i < chunkIdx; i++)
                {
                    byteIndex += _chunksLength[i];
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
            _chunksLength.Clear();
        }

        public DataChunks Copy(Allocator allocator)
        {
            CheckIsCreated();
            return new DataChunks(this, allocator);
        }

        public ReadOnlySpan<byte> GetChunkData(int chunkIndex)
        {
            CheckIsCreated();
            var chunkByteIndex = GetChunkByteIndex(chunkIndex);
            var chunkLength = _chunksLength[chunkIndex];
            return _data.AsArray().GetSubArray(chunkByteIndex, chunkLength).AsReadOnlySpan();
        }

        public NativeArray<byte> GetChunkData(int chunkIndex, Allocator allocator)
        {
            CheckIsCreated();
            var chunkByteIndex = GetChunkByteIndex(chunkIndex);
            var chunkLength = _chunksLength[chunkIndex];
            var chunkData = new NativeArray<byte>(chunkLength, allocator);
            _data.AsArray().GetSubArray(chunkByteIndex, chunkLength).CopyTo(chunkData);
            return chunkData;
        }

        public ReadOnlySpan<byte> GetChunksData(int chunkIndex, int count)
        {
            CheckIsCreated();
            var lastChunkIndex = chunkIndex + count - 1;
            var firstByteIndex = GetChunkByteIndex(chunkIndex);
            var lastByteIndex = GetChunkByteIndex(lastChunkIndex) + GetChunkLength(lastChunkIndex);
            var byteLength = lastByteIndex - firstByteIndex;
            return _data.AsArray().GetSubArray(firstByteIndex, byteLength).AsReadOnlySpan();
        }

        public NativeArray<byte> GetChunksData(int chunkIndex, int count, Allocator allocator)
        {
            CheckIsCreated();
            var lastChunkIndex = chunkIndex + count - 1;
            var firstByteIndex = GetChunkByteIndex(chunkIndex);
            var lastByteIndex = GetChunkByteIndex(lastChunkIndex) + GetChunkLength(lastChunkIndex);
            var byteLength = lastByteIndex - firstByteIndex;
            var chunksData = new NativeArray<byte>(byteLength, allocator);
            _data.AsArray().GetSubArray(firstByteIndex, byteLength).CopyTo(chunksData);
            return chunksData;
        }

        public ReadOnlySpan<byte> GetChunksData()
        {
            CheckIsCreated();
            return _data.AsArray().AsReadOnlySpan();
        }

        public NativeArray<byte> GetChunksData(Allocator allocator)
        {
            CheckIsCreated();
            return _data.ToArray(allocator);
        }

        public int GetChunkLength(int chunkIdx)
        {
            CheckIsCreated();
            return _chunksLength[chunkIdx];
        }

        public ReadOnlySpan<int> GetChunksLength(int chunkIndex, int count)
        {
            CheckIsCreated();
            return _chunksLength.AsArray().GetSubArray(chunkIndex, count).AsReadOnlySpan();
        }

        public NativeArray<int> GetChunksLength(int chunkIndex, int count, Allocator allocator)
        {
            CheckIsCreated();
            var chunksLength = new NativeArray<int>(count, allocator);
            _chunksLength.AsArray().GetSubArray(chunkIndex, count).CopyTo(chunksLength);
            return chunksLength;
        }

        public ReadOnlySpan<int> GetChunksLength()
        {
            CheckIsCreated();
            return _chunksLength.AsArray().AsReadOnlySpan();
        }

        public NativeArray<int> GetChunksLength(Allocator allocator)
        {
            CheckIsCreated();
            return _chunksLength.ToArray(allocator);
        }

        public enum ChunkPosition
        {
            Start,
            End
        }

        public override int GetHashCode()
        {
            var dataHash = _data.Length;
            var lengthHash = _chunksLength.Length;

            // ReSharper disable once ForCanBeConvertedToForeach
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (var i = 0; i < _data.Length; i++)
            {
                dataHash = unchecked(dataHash * 31 + _data[i]);
            }
            
            // ReSharper disable once ForCanBeConvertedToForeach
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (var i = 0; i < _chunksLength.Length; i++)
            {
                lengthHash = unchecked(lengthHash * 31 + _chunksLength[i]);
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
            if (ChunksTotalLength != other.ChunksTotalLength) return false;

            for (var i = 0; i < ChunksCount; i++)
            {
                if (_chunksLength[i] != other._chunksLength[i])
                    return false;
            }

            for (var i = 0; i < ChunksTotalLength; i++)
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
            _chunksLength.Dispose();
        }

        public override string ToString()
        {
            CheckIsCreated();

            var sb = new StringBuilder();

            sb.Append($"{ChunksCount} chunks, {_data.Length} bytes (");

            for (var i = 0; i < ChunksCount; i++)
            {
                sb.Append($"{_chunksLength[i]}");

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

        public bool IsCreated => _data.IsCreated && _chunksLength.IsCreated;
    }
}