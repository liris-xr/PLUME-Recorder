using System;
using System.Collections.Generic;
using System.Linq;

namespace PLUME.Core.Recorder.Data
{
    public class TimestampedDataChunks : IEqualityComparer<TimestampedDataChunks>
    {
        internal readonly DataChunks DataChunks = new();
        internal readonly List<long> Timestamps = new();

        public void Push(ReadOnlySpan<byte> data, long timestamp)
        {
            if (data.Length == 0)
                return;

            var chunkIndex = Timestamps.BinarySearch(timestamp);

            if (chunkIndex >= 0)
            {
                DataChunks.MergeIntoChunk(chunkIndex, data, DataChunks.ChunkPosition.End);
                return;
            }

            // In this case, chunk index is the bitwise complement of the index of the next element that is larger
            // than timestamp or, if there is no larger element,the bitwise complement of Count.
            DataChunks.Insert(~chunkIndex, data);
            Timestamps.Insert(~chunkIndex, timestamp);
        }

        public bool TryPopAll(DataChunks dataChunks, List<long> timestamps)
        {
            dataChunks.Clear();
            timestamps.Clear();

            if (DataChunks.ChunksCount == 0)
                return false;

            var chunksData = DataChunks.GetAllData();
            var chunksLengths = DataChunks.GetAllDataChunksLength();
            dataChunks.AddRange(chunksData, chunksLengths);
            timestamps.AddRange(Timestamps);
            DataChunks.Clear();
            Timestamps.Clear();
            return true;
        }

        public bool TryPopAllBeforeTimestamp(long timestamp, DataChunks dataChunks, List<long> timestamps,
            bool inclusive)
        {
            dataChunks.Clear();
            timestamps.Clear();

            if (DataChunks.ChunksCount == 0)
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

            // Pop from 0 to chunkIndex
            var chunksData = DataChunks.GetDataChunks(0, chunkIndex + 1);
            var chunksLengths = DataChunks.GetDataChunksLength(0, chunkIndex + 1);
            dataChunks.AddRange(chunksData, chunksLengths);
            DataChunks.RemoveRange(0, chunkIndex + 1);
            
            for(var i = 0; i <= chunkIndex; i++)
                timestamps.Add(Timestamps[i]);
            
            Timestamps.RemoveRange(0, chunkIndex + 1);
            return true;
        }

        public void Clear()
        {
            DataChunks.Clear();
            Timestamps.Clear();
        }

        public bool Equals(TimestampedDataChunks x, TimestampedDataChunks y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return Equals(x.DataChunks, y.DataChunks) && x.Timestamps.SequenceEqual(y.Timestamps);
        }

        public int GetHashCode(TimestampedDataChunks obj)
        {
            return HashCode.Combine(obj.DataChunks, obj.Timestamps);
        }

        public override bool Equals(object obj)
        {
            return obj is TimestampedDataChunks other && Equals(this, other);
        }

        public override int GetHashCode()
        {
            return GetHashCode(this);
        }

        public TimestampedDataChunks Copy()
        {
            var copy = new TimestampedDataChunks();
            copy.DataChunks.AddRange(DataChunks.GetAllData(), DataChunks.GetAllDataChunksLength());
            copy.Timestamps.AddRange(Timestamps);
            return copy;
        }

        public override string ToString()
        {
            return $"Chunks: {DataChunks}, Timestamps: ({string.Join(", ", Timestamps)})";
        }
    }
}