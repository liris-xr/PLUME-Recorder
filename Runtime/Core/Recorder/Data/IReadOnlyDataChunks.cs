using System;
using Unity.Collections;

namespace PLUME.Core.Recorder.Data
{
    public interface IReadOnlyDataChunks
    {
        public int ChunksCount { get; }
        
        public int DataLength { get; }

        public Span<byte> GetDataSpan(int chunkIndex);
        
        public Span<byte> GetDataSpan(int chunkIndex, int count);

        public Span<byte> GetDataSpan();

        public int GetLength(int chunkIdx);
        
        public Span<int> GetLengthsSpan(int chunkIndex, int count);
        
        public Span<int> GetLengthsSpan();
        
        public NativeArray<byte> GetData(int chunkIndex, Allocator allocator);
        
        public NativeArray<byte> GetData(int chunkIndex, int count, Allocator allocator);
        
        public NativeArray<byte> GetData(Allocator allocator);
        
        public NativeArray<int> GetLengths(int chunkIndex, int count, Allocator allocator);
        
        public NativeArray<int> GetLengths(Allocator allocator);
    }
}