using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Core.Recorder
{
    public static class SampleTypeUrlManager
    {
        private static int _nextId;
        // TODO: add multiple bin depending on the size of the string. This would enable burst compiled sample packing for most of the samples
        private static readonly Dictionary<FixedString128Bytes, SampleTypeUrlIndex> TypeUrlToIndex = new();
        private static readonly Dictionary<SampleTypeUrlIndex, FixedString128Bytes> IndexToTypeUrl = new();

        [BurstCompile]
        internal static void RegisterTypeUrl(FixedString128Bytes typeUrl)
        {
            if (TypeUrlToIndex.TryGetValue(typeUrl, out var index))
            {
                throw new InvalidOperationException($"TypeUrl {typeUrl} is already registered with index {index}");
            }

            index = new SampleTypeUrlIndex(_nextId++);
            TypeUrlToIndex.Add(typeUrl, index);
            IndexToTypeUrl.Add(index, typeUrl);
        }

        [BurstCompile]
        public static SampleTypeUrlIndex GetTypeUrlIndex(FixedString128Bytes typeUrl)
        {
            if (!TypeUrlToIndex.TryGetValue(typeUrl, out var index))
            {
                throw new InvalidOperationException($"TypeUrl {typeUrl} is not registered.");
            }

            return index;
        }

        [BurstCompile]
        public static FixedString128Bytes GetTypeUrlFromIndex(SampleTypeUrlIndex index)
        {
            if (!IndexToTypeUrl.TryGetValue(index, out var typeUrl))
            {
                throw new InvalidOperationException($"TypeUrlIndex {index} is not registered.");
            }

            return typeUrl;
        }
    }
}