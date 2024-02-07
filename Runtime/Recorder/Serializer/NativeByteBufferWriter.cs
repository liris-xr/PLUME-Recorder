using System;
using System.Buffers;
using Unity.Collections;

namespace PLUME.Recorder.Serializer
{
    public struct NativeByteBufferWriter : IByteBufferWriter, IDisposable
    {
        private readonly int _minBufferSize;
        private NativeList<byte> _data;
        private int _position;

        public NativeByteBufferWriter(Allocator allocator = Allocator.Temp, int minBufferSize = 256)
        {
            _minBufferSize = minBufferSize;
            _data = new NativeList<byte>(allocator);
            _position = 0;
        }

        public void Dispose()
        {
            _data.Dispose();
        }

        public void Advance(int count)
        {
            _position += count;
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            var size = sizeHint > _minBufferSize ? sizeHint : _minBufferSize;
            _data.Resize(_position + size, NativeArrayOptions.UninitializedMemory);
            return _data.AsArray().AsSpan().Slice(_position, size).ToArray().AsMemory();
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            var size = sizeHint > _minBufferSize ? sizeHint : _minBufferSize;
            _data.Resize(_position + size, NativeArrayOptions.UninitializedMemory);
            return _data.AsArray().AsSpan().Slice(_position, size);
        }

        public NativeArray<byte>.ReadOnly AsReadOnlyArray()
        {
            return _data.AsArray().AsReadOnly();
        }
    }
}