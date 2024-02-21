using System;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Core.Recorder
{
    [BurstCompile]
    [GenerateTestsForBurstCompatibility]
    public struct DataChunksTimestamped : IDisposable
    {
        internal DataChunks DataChunks;
        internal NativeList<long> Timestamps;

        public int ChunksCount => DataChunks.ChunksCount;
        public int DataLength => DataChunks.DataLength;

        public DataChunksTimestamped(Allocator allocator)
        {
            DataChunks = new DataChunks(allocator);
            Timestamps = new NativeList<long>(allocator);
        }

        public DataChunksTimestamped(ReadOnlySpan<byte> chunksData, ReadOnlySpan<int> chunksLength,
            ReadOnlySpan<long> timestamps, Allocator allocator)
        {
            DataChunks = new DataChunks(chunksData, chunksLength, allocator);
            Timestamps = new NativeList<long>(timestamps.Length, allocator);
            Timestamps.ResizeUninitialized(timestamps.Length);
            timestamps.CopyTo(Timestamps.AsArray().AsSpan());
        }

        public DataChunksTimestamped(DataChunksTimestamped other, Allocator allocator)
        {
            DataChunks = new DataChunks(other.DataChunks, allocator);
            Timestamps = new NativeList<long>(other.Timestamps.Length, allocator);
            Timestamps.AddRange(other.Timestamps.AsArray());
        }

        public void Add(DataChunks chunks, long timestamp)
        {
            CheckIsCreated();

            if (chunks.IsEmpty())
                return;

            var chunkIndex = Timestamps.BinarySearch(timestamp);

            if (chunkIndex >= 0)
            {
                DataChunks.MergeIntoChunk(chunkIndex, chunks.GetDataSpan(), DataChunks.ChunkPosition.End);
                return;
            }

            // In this case, chunk index is the bitwise complement of the index of the next element that is larger
            // than timestamp or, if there is no larger element, the bitwise complement of Count.
            var nearestChunkIndex = ~chunkIndex;
            DataChunks.Insert(nearestChunkIndex, chunks.GetDataSpan());
            Timestamps.InsertRange(nearestChunkIndex, 1);
            Timestamps[nearestChunkIndex] = timestamp;
        }

        public void Add(ReadOnlySpan<byte> chunkData, long timestamp)
        {
            CheckIsCreated();
            if (chunkData.Length == 0)
                return;

            if (Timestamps.IsEmpty || timestamp > Timestamps[^1])
            {
                DataChunks.Add(chunkData);
                Timestamps.Add(timestamp);
                return;
            }

            var chunkIndex = Timestamps.BinarySearch(timestamp);

            if (chunkIndex >= 0)
            {
                DataChunks.MergeIntoChunk(chunkIndex, chunkData, DataChunks.ChunkPosition.End);
                return;
            }

            // In this case, chunk index is the bitwise complement of the index of the next element that is larger
            // than timestamp or, if there is no larger element, the bitwise complement of Count.
            var nearestChunkIndex = ~chunkIndex;
            DataChunks.Insert(nearestChunkIndex, chunkData);
            Timestamps.InsertRange(nearestChunkIndex, 1);
            Timestamps[nearestChunkIndex] = timestamp;
        }

        public bool TryRemoveAll(DataChunksTimestamped dst)
        {
            CheckIsCreated();
            dst.Clear();

            if (DataChunks.IsEmpty())
                return false;

            dst.DataChunks.Add(DataChunks);
            dst.Timestamps.AddRange(Timestamps.AsArray());
            DataChunks.Clear();
            Timestamps.Clear();
            return true;
        }

        public bool TryRemoveAllBeforeTimestamp(long timestamp, DataChunksTimestamped dst, bool inclusive)
        {
            CheckIsCreated();
            dst.Clear();

            if (DataChunks.IsEmpty())
                return false;

            var chunkIndex = Timestamps.BinarySearch(timestamp);

            // Exact match, a chunk has the same timestamp
            if (chunkIndex >= 0)
            {
                if (!inclusive)
                    chunkIndex -= 1;
            }
            else
            {
                chunkIndex = ~chunkIndex - 1;
            }

            // Nothing to pop
            if (chunkIndex < 0)
                return false;

            var nRemovedChunks = chunkIndex + 1;
            var dataChunks = DataChunks.GetDataSpan(0, nRemovedChunks);
            var chunksLength = DataChunks.GetLengthsSpan(0, nRemovedChunks);
            var timestamps = Timestamps.AsArray().AsReadOnlySpan()[..nRemovedChunks];
            dst.DataChunks.AddRange(dataChunks, chunksLength);
            dst.Timestamps.InsertRange(0, nRemovedChunks);
            timestamps.CopyTo(dst.Timestamps.AsArray());

            DataChunks.RemoveRange(0, nRemovedChunks);
            Timestamps.RemoveRange(0, nRemovedChunks);
            return true;
        }

        public bool IsEmpty()
        {
            CheckIsCreated();
            return DataChunks.IsEmpty();
        }

        public void Clear()
        {
            CheckIsCreated();
            DataChunks.Clear();
            Timestamps.Clear();
        }

        public DataChunksTimestamped Copy(Allocator allocator)
        {
            CheckIsCreated();
            return new DataChunksTimestamped(this, allocator);
        }

        public override int GetHashCode()
        {
            int timestampHash;

            if (Timestamps.Length == 0)
            {
                timestampHash = -1;
            }
            else
            {
                unchecked
                {
                    timestampHash = 17;
                    timestampHash = 31 * timestampHash + Timestamps[0].GetHashCode();
                    timestampHash = 31 * timestampHash + Timestamps[Timestamps.Length / 2].GetHashCode();
                    timestampHash = 31 * timestampHash + Timestamps[^1].GetHashCode();
                    timestampHash = 31 * timestampHash + Timestamps.Length;
                }
            }

            return HashCode.Combine(DataChunks.GetHashCode(), timestampHash);
        }

        public override bool Equals(object obj)
        {
            if (!IsCreated)
                return false;
            if (obj is not DataChunksTimestamped other)
                return false;
            if (!other.IsCreated)
                return false;
            if (!DataChunks.Equals(other.DataChunks))
                return false;
            if (Timestamps.Length != other.Timestamps.Length)
                return false;

            // ReSharper disable once LoopCanBeConvertedToQuery
            for (var i = 0; i < Timestamps.Length; i++)
            {
                if (Timestamps[i] != other.Timestamps[i])
                    return false;
            }

            return true;
        }

        public void Dispose()
        {
            CheckIsCreated();
            DataChunks.Dispose();
            Timestamps.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckIsCreated()
        {
            if (!IsCreated)
                throw new InvalidOperationException($"{nameof(DataChunksTimestamped)} is not created.");
        }

        public bool IsCreated => DataChunks.IsCreated && Timestamps.IsCreated;

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"Chunks: {DataChunks}, Timestamps: (");

            for (var i = 0; i < Timestamps.Length; i++)
            {
                sb.Append(Timestamps[i]);
                if (i < Timestamps.Length - 1)
                    sb.Append(", ");
            }

            sb.Append(")");
            return sb.ToString();
        }

        public Span<byte> GetDataSpan(int chunkIndex)
        {
            return DataChunks.GetDataSpan(chunkIndex);
        }

        public Span<byte> GetDataSpan(int chunkIndex, int count)
        {
            return DataChunks.GetDataSpan(chunkIndex, count);
        }

        public Span<byte> GetDataSpan()
        {
            return DataChunks.GetDataSpan();
        }

        public int GetLength(int chunkIdx)
        {
            return DataChunks.GetLength(chunkIdx);
        }

        public Span<int> GetLengthsSpan(int chunkIndex, int count)
        {
            return DataChunks.GetLengthsSpan(chunkIndex, count);
        }

        public Span<int> GetLengthsSpan()
        {
            return DataChunks.GetLengthsSpan();
        }

        public NativeArray<byte> GetData(int chunkIndex, Allocator allocator)
        {
            return DataChunks.GetData(chunkIndex, allocator);
        }

        public NativeArray<byte> GetData(int chunkIndex, int count, Allocator allocator)
        {
            return DataChunks.GetData(chunkIndex, count, allocator);
        }

        public NativeArray<byte> GetData(Allocator allocator)
        {
            return DataChunks.GetData(allocator);
        }

        public NativeArray<int> GetLengths(int chunkIndex, int count, Allocator allocator)
        {
            return DataChunks.GetLengths(chunkIndex, count, allocator);
        }

        public NativeArray<int> GetLengths(Allocator allocator)
        {
            return DataChunks.GetLengths(allocator);
        }

        public static implicit operator ReadOnlySpan<byte>(DataChunksTimestamped dataChunksTimestamped)
        {
            return dataChunksTimestamped.GetDataSpan();
        }
    }
}