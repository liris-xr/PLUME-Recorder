using System;
using Unity.Collections;

namespace PLUME.Core.Recorder.Data
{
    public interface IReadOnlyDataChunks
    {
        public int ChunksCount { get; }
        
        public int ChunksTotalLength { get; }

        public ReadOnlySpan<byte> GetChunkData(int chunkIndex);
        
        public ReadOnlySpan<byte> GetChunksData(int chunkIndex, int count);

        public ReadOnlySpan<byte> GetChunksData();

        public int GetChunkLength(int chunkIdx);
        
        public ReadOnlySpan<int> GetChunksLength(int chunkIndex, int count);
        
        public ReadOnlySpan<int> GetChunksLength();
        
        public NativeArray<byte> GetChunkData(int chunkIndex, Allocator allocator);
        
        public NativeArray<byte> GetChunksData(int chunkIndex, int count, Allocator allocator);
        
        public NativeArray<byte> GetChunksData(Allocator allocator);
        
        public NativeArray<int> GetChunksLength(int chunkIndex, int count, Allocator allocator);
        
        public NativeArray<int> GetChunksLength(Allocator allocator);
    }
}