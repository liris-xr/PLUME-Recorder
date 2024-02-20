using System.Collections.Generic;
using Google.Protobuf;
using PLUME.Core.Recorder.Module;
using ProtoBurst;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Core.Recorder.Data
{
    public struct FrameDataWriter
    {
        private DataChunks _serializedSamplesData;
        private DataChunks _serializedSamplesTypeUrl;

        public FrameDataWriter(DataChunks serializedSamplesData, DataChunks serializedSamplesTypeUrl)
        {
            _serializedSamplesData = serializedSamplesData;
            _serializedSamplesTypeUrl = serializedSamplesTypeUrl;
        }

        public void WriteSerializedBatch(SampleTypeUrl sampleTypeUrl, DataChunks serializedSamples)
        {
            _serializedSamplesData.Add(serializedSamples);
            _serializedSamplesTypeUrl.AddReplicate(sampleTypeUrl, serializedSamples.ChunksCount);
        }

        public void WriteBatch<TU, TV>(NativeArray<TU> samples, TV batchSerializer)
            where TU : unmanaged, IProtoBurstMessage
            where TV : ISampleBatchSerializer<TU>
        {
            if (samples.Length == 0)
                return;

            var sampleTypeUrl = samples[0].GetTypeUrl(Allocator.Persistent);
            var serializedSamples = batchSerializer.SerializeBatch(samples, Allocator.Persistent);
            WriteSerializedBatch(sampleTypeUrl, serializedSamples);
            sampleTypeUrl.Dispose();
            serializedSamples.Dispose();
        }

        // ReSharper restore Unity.ExpensiveCode

        public void WriteBatch<T>(NativeArray<T> samples) where T : unmanaged, IProtoBurstMessage
        {
            if (samples.Length == 0)
                return;

            var sampleTypeUrl = samples[0].GetTypeUrl(Allocator.Persistent);
            var serializedSamples = new DataChunks(Allocator.Persistent);

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < samples.Length; i++)
            {
                var sample = samples[i];
                var size = sample.ComputeSize();
                var data = new NativeList<byte>(size, Allocator.Persistent);
                var buffer = new BufferWriter(data);

                sample.WriteTo(ref buffer);
                serializedSamples.Add(data.AsArray());
                data.Dispose();
            }

            WriteSerializedBatch(sampleTypeUrl, serializedSamples);
            serializedSamples.Dispose();
            sampleTypeUrl.Dispose();
        }

        public void Write<T>(T sample) where T : unmanaged, IProtoBurstMessage
        {
            var sampleTypeUrl = sample.GetTypeUrl(Allocator.Persistent);
            var serializedSample = new NativeList<byte>(sample.ComputeSize(), Allocator.Persistent);
            var buffer = new BufferWriter(serializedSample);

            sample.WriteTo(ref buffer);
            _serializedSamplesData.Add(serializedSample.AsArray());
            _serializedSamplesTypeUrl.Add(sampleTypeUrl);

            sampleTypeUrl.Dispose();
            serializedSample.Dispose();
        }

        // [BurstDiscard]
        // // ReSharper restore Unity.ExpensiveCode
        // public void WriteBatch(IList<IMessage> samples)
        // {
        //     if (samples.Count == 0)
        //         return;
        //
        //     var sampleTypeUrl = SampleTypeUrl.Alloc(samples[0].Descriptor, Allocator.Persistent);
        //     var serializedSamples = new DataChunks(Allocator.Persistent);
        //
        //     // ReSharper disable once ForCanBeConvertedToForeach
        //     for (var i = 0; i < samples.Count; i++)
        //     {
        //         var sample = samples[i];
        //         var data = new NativeList<byte>(sample.CalculateSize(), Allocator.Persistent);
        //
        //         sample.WriteTo(data.AsArray().AsSpan());
        //         serializedSamples.Add(data.AsArray());
        //         data.Dispose();
        //     }
        //
        //     WriteSerializedBatch(sampleTypeUrl, serializedSamples);
        //     serializedSamples.Dispose();
        //     sampleTypeUrl.Dispose();
        // }
        //
        // [BurstDiscard]
        // // ReSharper restore Unity.ExpensiveCode
        // public void Write(IMessage sample)
        // {
        //     var sampleTypeUrl = SampleTypeUrl.Alloc(sample.Descriptor, Allocator.Persistent);
        //     var serializedSample = new NativeList<byte>(sample.CalculateSize(), Allocator.Persistent);
        //
        //     sample.WriteTo(serializedSample.AsArray().AsSpan());
        //     _serializedSamplesData.Add(serializedSample.AsArray());
        //     _serializedSamplesTypeUrl.Add(sampleTypeUrl.Bytes);
        //
        //     sampleTypeUrl.Dispose();
        //     serializedSample.Dispose();
        // }
    }
}