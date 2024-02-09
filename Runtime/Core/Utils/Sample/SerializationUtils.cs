using System;
using Google.Protobuf;
using PLUME.Core.Recorder;
using Unity.Collections;

namespace PLUME.Core.Utils.Sample
{
    public static class SerializationUtils
    {
        public static unsafe void SerializeSampleToBuffer(this IMessage message, SampleTypeUrlIndex sampleTypeUrlIndex,
            FrameDataBuffer buffer)
        {
            var size = message.CalculateSize();
            Span<byte> bytes = stackalloc byte[size];
            message.WriteTo(bytes);
            buffer.AddSerializedSample(sampleTypeUrlIndex, bytes);
        }

        public static unsafe Span<byte> ToSpan(this IMessage message)
        {
            var size = message.CalculateSize();
            var bytes = stackalloc byte[size];
            var span = new Span<byte>(bytes, size);
            message.WriteTo(span);
            return span;
        }

        public static unsafe NativeArray<byte> ToNativeArray(this IMessage message, Allocator allocator)
        {
            var size = message.CalculateSize();
            var list = new NativeList<byte>(size, allocator);
            var bytes = stackalloc byte[size];
            var span = new Span<byte>(bytes, size);
            message.WriteTo(span);
            list.Resize(size, NativeArrayOptions.UninitializedMemory);
            span.CopyTo(list.AsArray().AsSpan());
            return list.AsArray();
        }
    }
}