using System;
using Google.Protobuf;
using Unity.Collections;

namespace PLUME.Core.Utils
{
    public static class SampleSerializationUtils
    {
        public static unsafe void Serialize(this IMessage message, ref NativeList<byte> buffer)
        {
            var size = message.CalculateSize();
            var bytes = stackalloc byte[size];
            var span = new Span<byte>(bytes, size);
            message.WriteTo(span);

            fixed (byte* ptr = span)
            {
                buffer.AddRange(ptr, size);
            }
        }

        public static NativeArray<byte> Serialize(this IMessage message, Allocator allocator)
        {
            var bytes = new NativeList<byte>(message.CalculateSize(), Allocator.Persistent);
            message.Serialize(ref bytes);
            var bytesArray = bytes.ToArray(allocator);
            bytes.Dispose();
            return bytesArray;
        }
    }
}