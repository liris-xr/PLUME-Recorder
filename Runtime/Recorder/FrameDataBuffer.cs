using System;
using Unity.Collections;

namespace PLUME.Recorder
{
    public struct FrameDataBuffer : IDisposable
    {
        internal NativeList<byte> Data;
        internal NativeList<int> Lengths;
        internal NativeList<TypeUrlIndex> TypeUrlIndices;

        public FrameDataBuffer(Allocator allocator)
        {
            Data = new NativeList<byte>(allocator);
            Lengths = new NativeList<int>(allocator);
            TypeUrlIndices = new NativeList<TypeUrlIndex>(allocator);
        }

        public void Merge(FrameDataBuffer other)
        {
            Data.AddRange(other.Data.AsArray());
            Lengths.AddRange(other.Lengths.AsArray());
            TypeUrlIndices.AddRange(other.TypeUrlIndices.AsArray());
        }

        public void AddSerializedSample(TypeUrlIndex typeUrlIndex, ReadOnlySpan<byte> data)
        {
            unsafe
            {
                fixed (byte* ptr = data)
                {
                    Data.AddRange(ptr, data.Length);
                }

                Lengths.Add(data.Length);
                TypeUrlIndices.Add(typeUrlIndex);
            }
        }

        public void AddSerializedSamples(TypeUrlIndex typeUrlIndex, ReadOnlySpan<byte> data, ReadOnlySpan<int> lengths)
        {
            unsafe
            {
                fixed (byte* ptr = data)
                {
                    Data.AddRange(ptr, data.Length);
                }

                // TODO: in case a chunk misreports its length, we should check that the sum of lengths is equal to the data length
                // TODO: we should do this when packing the frame data because then we can sum the lengths as we go
                fixed (int* ptr = lengths)
                {
                    Lengths.AddRange(ptr, lengths.Length);
                }

                TypeUrlIndices.AddReplicate(typeUrlIndex, lengths.Length);
            }
        }

        public void AddSerializedSamples(ReadOnlySpan<TypeUrlIndex> typeUrlIndices, ReadOnlySpan<byte> data,
            ReadOnlySpan<int> lengths)
        {
            if (typeUrlIndices.Length != lengths.Length)
                throw new ArgumentException(
                    $"{nameof(typeUrlIndices)} and {nameof(lengths)} must have the same length.");

            unsafe
            {
                fixed (byte* ptr = data)
                {
                    Data.AddRange(ptr, data.Length);
                }

                fixed (int* ptr = lengths)
                {
                    Lengths.AddRange(ptr, lengths.Length);
                }

                fixed (TypeUrlIndex* ptr = typeUrlIndices)
                {
                    TypeUrlIndices.AddRange(ptr, typeUrlIndices.Length);
                }
            }
        }

        public void Dispose()
        {
            Data.Dispose();
            Lengths.Dispose();
            TypeUrlIndices.Dispose();
        }
    }
}