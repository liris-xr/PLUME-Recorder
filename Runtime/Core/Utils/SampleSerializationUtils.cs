using Google.Protobuf;
using ProtoBurst;
using Unity.Collections;

namespace PLUME.Core.Utils
{
    public static class SampleSerializationUtils
    {
        // public static NativeArray<byte> SerializeLengthPrefixed(this IMessage message, Allocator allocator)
        // {
        //     var size = message.CalculateSize();
        //     var tmpBytes = new NativeList<byte>(Allocator.Persistent);
        //     tmpBytes.ResizeUninitialized(size + WritingPrimitives.LengthPrefixMaxSize);
        //     message.WriteTo((System.Span<byte>)tmpBytes.AsArray().AsSpan());
        //     WritingPrimitives.InsertLength(size, ref tmpBytes, 0);
        //     var bytes = tmpBytes.ToArray(allocator);
        //     tmpBytes.Dispose();
        //     return bytes;
        // }
        //
        // public static NativeArray<byte> Serialize(this IMessage message, Allocator allocator)
        // {
        //     var size = message.CalculateSize();
        //     var bytes = new NativeArray<byte>(size, allocator);
        //     message.WriteTo((System.Span<byte>)bytes.AsSpan());
        //     return bytes;
        // }
    }
}