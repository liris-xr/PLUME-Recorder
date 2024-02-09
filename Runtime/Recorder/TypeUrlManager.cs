using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Recorder
{
    public static class TypeUrlManager
    {
        private static int _nextId;
        // TODO: add multiple bin depending on the size of the string. This would enable burst compiled sample packing for most of the samples
        private static readonly Dictionary<FixedString128Bytes, TypeUrlIndex> TypeUrlToIndex = new();
        private static readonly Dictionary<TypeUrlIndex, FixedString128Bytes> IndexToTypeUrl = new();

        [BurstCompile]
        internal static void RegisterTypeUrl(FixedString128Bytes typeUrl)
        {
            if (TypeUrlToIndex.TryGetValue(typeUrl, out var index))
            {
                throw new InvalidOperationException($"TypeUrl {typeUrl} is already registered with index {index}");
            }

            index = new TypeUrlIndex(_nextId++);
            TypeUrlToIndex.Add(typeUrl, index);
            IndexToTypeUrl.Add(index, typeUrl);
        }

        [BurstCompile]
        public static TypeUrlIndex GetTypeUrlIndex(FixedString128Bytes typeUrl)
        {
            if (!TypeUrlToIndex.TryGetValue(typeUrl, out var index))
            {
                throw new InvalidOperationException($"TypeUrl {typeUrl} is not registered.");
            }

            return index;
        }

        [BurstCompile]
        public static FixedString128Bytes GetTypeUrlFromIndex(TypeUrlIndex index)
        {
            if (!IndexToTypeUrl.TryGetValue(index, out var typeUrl))
            {
                throw new InvalidOperationException($"TypeUrlIndex {index} is not registered.");
            }

            return typeUrl;
        }
    }
}