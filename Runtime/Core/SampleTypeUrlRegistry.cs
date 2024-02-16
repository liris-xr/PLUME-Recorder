using System;
using System.Linq;
using Google.Protobuf.Reflection;
using PLUME.Core.Recorder;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Core
{
    // TODO: don't use NativeHashMap, use a normal HashMap, the systems requiring an access to a blittable type should get the index first and pass it as the blittable type
    public struct SampleTypeUrlRegistry : IDisposable
    {
        private NativeHashMap<FixedString128Bytes, SampleTypeUrlIndex> _typeUrlToIndex;
        private NativeHashMap<SampleTypeUrlIndex, FixedString128Bytes> _indexToTypeUrl;

        public SampleTypeUrlRegistry(Allocator allocator)
        {
            _typeUrlToIndex = new NativeHashMap<FixedString128Bytes, SampleTypeUrlIndex>(100, allocator);
            _indexToTypeUrl = new NativeHashMap<SampleTypeUrlIndex, FixedString128Bytes>(100, allocator);
        }

        private int NextTypeUrlIndex()
        {
            var indices = _indexToTypeUrl.GetKeyArray(Allocator.Temp);

            if (indices.Length == 0)
            {
                indices.Dispose();
                return 0;
            }

            var index = indices.Max(m => m.Index) + 1;
            indices.Dispose();
            return index;
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

            var newTypeUrlIndex = NextTypeUrlIndex();
            index = new SampleTypeUrlIndex(newTypeUrlIndex);
            _typeUrlToIndex.Add(fixedString, index);
            _indexToTypeUrl.Add(index, fixedString);
            return index;
        }

        [BurstCompile]
        public FixedString128Bytes GetTypeUrlFromIndex(SampleTypeUrlIndex index)
        {
            if (!_indexToTypeUrl.TryGetValue(index, out var typeUrl))
            {
                throw new InvalidOperationException($"TypeUrlIndex {index.Index} is not registered.");
            }

            return typeUrl;
        }

        public void Dispose()
        {
            _typeUrlToIndex.Dispose();
            _indexToTypeUrl.Dispose();
        }

        public bool IsCreated => _typeUrlToIndex.IsCreated && _indexToTypeUrl.IsCreated;
    }
}