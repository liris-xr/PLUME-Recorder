using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Google.Protobuf;
using PLUME.Core.Utils;
using ProtoBurst;
using ProtoBurst.Message;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Core.Recorder.Data
{
    // TODO: make it a FrameDataWriter, containing the DataChunks to write to. Automatically write the Any tag and the length of the data to write
    // TODO: store the sample type url as bytes directly for faster serialization
    [BurstCompile]
    public struct FrameDataWriter
    {
        private DataChunks _dataChunks;

        public FrameDataWriter(DataChunks dataChunks)
        {
            _dataChunks = dataChunks;
        }

        [BurstDiscard]
        // ReSharper restore Unity.ExpensiveCode
        public void Write(IList<IMessage> samples)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var index = 0; index < samples.Count; index++)
            {
                var sample = samples[index];
                var sampleTypeUrl = SampleTypeUrlRegistry.GetOrCreate(sample.Descriptor);
                Write(sample, sampleTypeUrl);
            }
        }

        [BurstDiscard]
        // ReSharper restore Unity.ExpensiveCode
        public void Write(IList<IMessage> samples, SampleTypeUrl typeUrl)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var index = 0; index < samples.Count; index++)
            {
                var sample = samples[index];
                Write(sample, typeUrl);
            }
        }

        [BurstDiscard]
        // ReSharper restore Unity.ExpensiveCode
        public void Write(IList<IMessage> samples, IList<SampleTypeUrl> sampleTypeUrls)
        {
            if (samples.Count != sampleTypeUrls.Count)
            {
                throw new ArgumentException("The number of samples and sample type urls must be the same.");
            }

            for (var i = 0; i < samples.Count; i++)
            {
                Write(samples[i], sampleTypeUrls[i]);
            }
        }

        public void Write<T>(NativeList<T> samples) where T : unmanaged, IProtoBurstMessage
        {
            Write(samples.AsArray());
        }
        
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(NativeArray<T> samples) where T : unmanaged, IProtoBurstMessage
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < samples.Length; i++)
            {
                Write(samples[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(T sample) where T : unmanaged, IProtoBurstMessage
        {
            var bytes = sample.Serialize(Allocator.TempJob);
            Write(bytes.AsArray(), sample.TypeUrl);
            bytes.Dispose();
        }

        [BurstDiscard]
        // ReSharper restore Unity.ExpensiveCode
        public void Write(IMessage sample, SampleTypeUrl typeUrl)
        {
            var bytes = sample.Serialize(Allocator.Persistent);
            Write(bytes.AsReadOnlySpan(), typeUrl);
            bytes.Dispose();
        }
        
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ReadOnlySpan<byte> value, SampleTypeUrl typeUrl)
        {
            var any = Any.Pack(value, typeUrl, Allocator.TempJob);
            var bytes = any.SerializeLengthPrefixed(Allocator.TempJob);
            _dataChunks.Add(bytes.AsArray());
            bytes.Dispose();
            any.Dispose();
        }
    }
}