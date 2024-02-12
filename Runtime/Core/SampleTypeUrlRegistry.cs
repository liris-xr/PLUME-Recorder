using System;
using Google.Protobuf.Reflection;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Core
{
    public class SampleTypeUrlRegistry : IDisposable
    {
        private static SampleTypeUrlRegistry _instance;

        private int _nextTypeUrlIndex;
        private NativeHashMap<FixedString128Bytes, SampleTypeUrlIndex> _typeUrlToIndex;
        private NativeHashMap<SampleTypeUrlIndex, FixedString128Bytes> _indexToTypeUrl;

        private SampleTypeUrlRegistry()
        {
            _typeUrlToIndex = new NativeHashMap<FixedString128Bytes, SampleTypeUrlIndex>(100, Allocator.Persistent);
            _indexToTypeUrl = new NativeHashMap<SampleTypeUrlIndex, FixedString128Bytes>(100, Allocator.Persistent);
        }

        ~SampleTypeUrlRegistry()
        {
            Dispose();
        }

        public SampleTypeUrlIndex GetOrCreateTypeUrlIndex(string prefix, MessageDescriptor descriptor)
        {
            var typeUrl = !prefix.EndsWith("/") ? prefix + "/" + descriptor.FullName : prefix + descriptor.FullName;
            return GetOrCreateTypeUrlIndex(typeUrl);
        }
        
        public SampleTypeUrlIndex GetOrCreateTypeUrlIndex(string typeUrl)
        {
            var fixedString = new FixedString128Bytes();
            var error = fixedString.CopyFromTruncated(typeUrl);

            if (error == CopyError.Truncation)
            {
                throw new InvalidOperationException(
                    $"TypeUrl {typeUrl} is too long (max length is {FixedString128Bytes.UTF8MaxLengthInBytes} UTF-8 bytes)");
            }

            if (_typeUrlToIndex.TryGetValue(fixedString, out var index))
            {
                return index;
            }

            index = new SampleTypeUrlIndex(_nextTypeUrlIndex++);
            _typeUrlToIndex.Add(fixedString, index);
            _indexToTypeUrl.Add(index, fixedString);
            return index;
        }

        [BurstCompile]
        public FixedString128Bytes GetTypeUrlFromIndex(SampleTypeUrlIndex index)
        {
            if (!_indexToTypeUrl.TryGetValue(index, out var typeUrl))
            {
                throw new InvalidOperationException($"TypeUrlIndex {index} is not registered.");
            }

            return typeUrl;
        }

        public void Dispose()
        {
            _typeUrlToIndex.Dispose();
            _indexToTypeUrl.Dispose();
        }

        public static SampleTypeUrlRegistry Instance => _instance ??= new SampleTypeUrlRegistry();
    }
}