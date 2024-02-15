using System;

namespace PLUME.Core.Recorder.Data
{
    public class DataChunks
    {
        private byte[] _chunksData;
        private int[] _chunksDataLength;

        public int ChunksDataCapacity => _chunksData.Length;
        public int ChunksLengthCapacity => _chunksDataLength.Length;
        public int ChunksCount { get; private set; }
        public int TotalDataLength { get; private set; }

        public DataChunks(int initialBytesCapacity = 1024, int initialChunksCapacity = 16)
        {
            _chunksData = new byte[initialBytesCapacity];
            _chunksDataLength = new int[initialChunksCapacity];
        }

        public void AddDataChunk(ReadOnlySpan<byte> chunk)
        {
            EnsureChunksDataCapacity(TotalDataLength + chunk.Length);
            EnsureChunksLengthCapacity(ChunksCount + 1);

            chunk.CopyTo(new Span<byte>(_chunksData, TotalDataLength, chunk.Length));
            _chunksDataLength[ChunksCount] = chunk.Length;

            TotalDataLength += chunk.Length;
            ChunksCount += 1;
        }
        
        public void AddDataChunks(ReadOnlySpan<byte> chunksData, ReadOnlySpan<int> chunksLengths)
        {
            EnsureChunksDataCapacity(TotalDataLength + chunksData.Length);
            EnsureChunksLengthCapacity(ChunksCount + chunksLengths.Length);

            chunksData.CopyTo(new Span<byte>(_chunksData, TotalDataLength, chunksData.Length));
            chunksLengths.CopyTo(new Span<int>(_chunksDataLength, ChunksCount, chunksLengths.Length));

            TotalDataLength += chunksData.Length;
            ChunksCount += chunksLengths.Length;
        }

        public void InsertDataChunk(int chunkIndex, ReadOnlySpan<byte> chunk)
        {
            EnsureChunksDataCapacity(TotalDataLength + chunk.Length);
            EnsureChunksLengthCapacity(ChunksCount + 1);

            var byteIndex = GetChunkByteIndex(chunkIndex);

            Array.Copy(_chunksData, byteIndex, _chunksData, byteIndex + chunk.Length,
                TotalDataLength - byteIndex);
            chunk.CopyTo(new Span<byte>(_chunksData, byteIndex, chunk.Length));

            Array.Copy(_chunksDataLength, chunkIndex, _chunksDataLength, chunkIndex + 1, ChunksCount - chunkIndex);

            _chunksDataLength[chunkIndex] = chunk.Length;
            TotalDataLength += chunk.Length;
            ChunksCount += 1;
        }

        /// <summary>
        /// Merge the data with the chunk at the given index. The data is merged at the end of the chunk.
        /// </summary>
        /// <param name="chunkIndex"></param>
        /// <param name="chunk"></param>
        /// <param name="pos"></param>
        public void MergeIntoChunk(int chunkIndex, ReadOnlySpan<byte> chunk, ChunkPosition pos)
        {
            var byteIndex = pos == ChunkPosition.Start
                ? GetChunkByteIndex(chunkIndex)
                : GetChunkByteIndex(chunkIndex) + GetChunkDataLength(chunkIndex);

            Array.Copy(_chunksData, byteIndex, _chunksData, byteIndex + chunk.Length, TotalDataLength - byteIndex);
            chunk.CopyTo(new Span<byte>(_chunksData, byteIndex, chunk.Length));

            _chunksDataLength[chunkIndex] += chunk.Length;
            TotalDataLength += chunk.Length;
        }

        public void RemoveDataChunk(int chunkIndex)
        {
            var byteIndex = GetChunkByteIndex(chunkIndex);
            var chunkLength = _chunksDataLength[chunkIndex];
            Array.Copy(_chunksData, byteIndex + chunkLength, _chunksData, byteIndex,
                TotalDataLength - byteIndex - chunkLength);
            Array.Copy(_chunksDataLength, chunkIndex + 1, _chunksDataLength, chunkIndex, ChunksCount - chunkIndex - 1);
            TotalDataLength -= chunkLength;
            ChunksCount -= 1;
        }

        public void RemoveDataChunks(int chunkIndex, int count)
        {
            var lastChunkIndex = chunkIndex + count - 1;
            var firstByteIndex = GetChunkByteIndex(chunkIndex);
            var lastByteIndex = GetChunkByteIndex(lastChunkIndex) + _chunksDataLength[lastChunkIndex];

            Array.Copy(_chunksData, lastByteIndex, _chunksData, firstByteIndex, TotalDataLength - lastByteIndex);
            Array.Copy(_chunksDataLength, lastChunkIndex + 1, _chunksDataLength, chunkIndex,
                ChunksCount - lastChunkIndex - 1);
            TotalDataLength -= lastByteIndex - firstByteIndex;
            ChunksCount -= count;
        }
        
        public void Clear()
        {
            ChunksCount = 0;
            TotalDataLength = 0;
        }
        
        public ReadOnlySpan<byte> GetChunkData(int chunkIndex)
        {
            var chunkByteIndex = GetChunkByteIndex(chunkIndex);
            var chunkLength = _chunksDataLength[chunkIndex];
            return new ReadOnlySpan<byte>(_chunksData, chunkByteIndex, chunkLength);
        }

        public ReadOnlySpan<byte> GetChunksData(int chunkIndex, int count)
        {
            var lastChunkIndex = chunkIndex + count - 1;
            var firstByteIndex = GetChunkByteIndex(chunkIndex);
            var lastByteIndex = GetChunkByteIndex(lastChunkIndex) + _chunksDataLength[lastChunkIndex];
            return new ReadOnlySpan<byte>(_chunksData, firstByteIndex, lastByteIndex - firstByteIndex);
        }

        public int GetChunkDataLength(int chunkIdx)
        {
            return _chunksDataLength[chunkIdx];
        }
        
        public ReadOnlySpan<int> GetChunksDataLength(int chunkIndex, int count)
        {
            return new ReadOnlySpan<int>(_chunksDataLength, chunkIndex, count);
        }
        
        public ReadOnlySpan<byte> GetAllChunksData()
        {
            return new ReadOnlySpan<byte>(_chunksData, 0, TotalDataLength);
        }

        public ReadOnlySpan<int> GetAllChunksDataLength()
        {
            return new ReadOnlySpan<int>(_chunksDataLength, 0, ChunksCount);
        }

        public int GetChunkByteIndex(int chunkIdx)
        {
            // Perform a cumulative sum of the chunk lengths.
            // We either start summing from the start or the end of the data to minimize the number of iterations.
            if (chunkIdx > ChunksCount / 2)
            {
                var byteIndex = TotalDataLength;

                for (var i = ChunksCount - 1; i >= chunkIdx; i--)
                {
                    byteIndex -= _chunksDataLength[i];
                }

                return byteIndex;
            }
            else
            {
                var byteIndex = 0;

                for (var i = 0; i < chunkIdx; i++)
                {
                    byteIndex += _chunksDataLength[i];
                }

                return byteIndex;
            }
        }

        private void EnsureChunksDataCapacity(int requestedCapacity)
        {
            if (requestedCapacity <= ChunksDataCapacity) return;
            var newCapacity = Math.Max(requestedCapacity, ChunksDataCapacity * 2);
            Array.Resize(ref _chunksData, newCapacity);
        }

        private void EnsureChunksLengthCapacity(int requestedCapacity)
        {
            if (requestedCapacity <= ChunksLengthCapacity) return;
            var newCapacity = Math.Max(requestedCapacity, ChunksLengthCapacity * 2);
            Array.Resize(ref _chunksDataLength, newCapacity);
        }

        public enum ChunkPosition
        {
            Start,
            End
        }
    }
}