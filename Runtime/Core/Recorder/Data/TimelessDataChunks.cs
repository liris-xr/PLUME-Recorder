using System;
using System.Collections.Generic;

namespace PLUME.Core.Recorder.Data
{
    public class TimelessDataChunks : IEqualityComparer<TimelessDataChunks>
    {
        internal readonly DataChunks DataChunks = new();

        public void Push(ReadOnlySpan<byte> data)
        {
            if (data.Length == 0)
                return;

            DataChunks.Add(data);
        }

        public bool TryPopAll(DataChunks dataChunks)
        {
            dataChunks.Clear();

            if (DataChunks.ChunksCount == 0)
                return false;

            var chunksData = DataChunks.GetAllData();
            var chunksLengths = DataChunks.GetAllDataChunksLength();
            dataChunks.AddRange(chunksData, chunksLengths);
            DataChunks.Clear();
            return true;
        }
        
        public void Clear()
        {
            DataChunks.Clear();
        }

        public bool Equals(TimelessDataChunks x, TimelessDataChunks y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.DataChunks.Equals(y.DataChunks);
        }

        public int GetHashCode(TimelessDataChunks obj)
        {
            return DataChunks.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is TimelessDataChunks other && Equals(this, other);
        }

        public override int GetHashCode()
        {
            return GetHashCode(this);
        }

        public TimelessDataChunks Copy()
        {
            var copy = new TimelessDataChunks();
            copy.DataChunks.AddRange(DataChunks.GetAllData(), DataChunks.GetAllDataChunksLength());
            return copy;
        }
        
        public override string ToString()
        {
            return $"Chunks: {DataChunks}";
        }
    }
}