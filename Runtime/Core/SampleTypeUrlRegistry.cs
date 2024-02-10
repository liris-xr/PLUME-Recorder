using System;
using System.Collections.Generic;
using Google.Protobuf.Reflection;
using Unity.Collections;

namespace PLUME.Core
{
    public static class SampleTypeUrlRegistry
    {
        private static int _nextTypeUrlIndex;
        
        private static readonly Dictionary<FixedString128Bytes, SampleTypeUrlIndex> TypeUrlToIndex = new();
        private static readonly Dictionary<SampleTypeUrlIndex, FixedString128Bytes> IndexToTypeUrl = new();

        public static SampleTypeUrlIndex GetOrCreateTypeUrlIndex(string prefix, MessageDescriptor descriptor)
        {
            var typeUrl = !prefix.EndsWith("/") ? prefix + "/" + descriptor.FullName : prefix + descriptor.FullName;
            return GetOrCreateTypeUrlIndex(typeUrl);
        }

        public static SampleTypeUrlIndex GetOrCreateTypeUrlIndex(string typeUrl)
        {
            var fixedString = new FixedString128Bytes();
            var error = fixedString.CopyFromTruncated(typeUrl);

            if (error == CopyError.Truncation)
            {
                throw new InvalidOperationException(
                    $"TypeUrl {typeUrl} is too long (max length is {FixedString128Bytes.UTF8MaxLengthInBytes} UTF-8 bytes)");
            }

            if (TypeUrlToIndex.TryGetValue(fixedString, out var index))
            {
                return index;
            }

            index = new SampleTypeUrlIndex(_nextTypeUrlIndex++);
            TypeUrlToIndex.Add(fixedString, index);
            IndexToTypeUrl.Add(index, fixedString);
            return index;
        }

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