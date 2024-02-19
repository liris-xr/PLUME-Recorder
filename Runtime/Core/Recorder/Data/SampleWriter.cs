using System.Collections.Generic;
using Google.Protobuf;
using PLUME.Core.Recorder.Module;
using ProtoBurst;
using ProtoBurst.Message;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Core.Recorder.Data
{
    public struct SampleWriter
    {
        private NativeList<byte> _dataDst;

        public SampleWriter(NativeList<byte> dataDst)
        {
            _dataDst = dataDst;
        }

        public void WriteBatch<TU, TV>(NativeList<TU> samples, TV batchSerializer)
            where TU : unmanaged, IProtoBurstMessage
            where TV : unmanaged, ISampleBatchSerializer<TU>
        {
            if (samples.Length == 0)
                return;

            var bytes = batchSerializer.SerializeBatch(samples, Allocator.Persistent);
            _dataDst.AddRange(bytes.AsArray());
            bytes.Dispose();
        }

        public void Write<T>(T sample) where T : unmanaged, IProtoBurstMessage
        {
            var sampleTypeUrl = sample.GetTypeUrl(Allocator.Persistent);
            var sampleSize = sample.ComputeSize();
            var packedSampleSize = Any.ComputeSize(sampleTypeUrl.BytesLength, sampleSize);
            var buffer = new BufferWriter(packedSampleSize, Allocator.Persistent);
            Any.WriteTo(ref sampleTypeUrl, ref sample, ref buffer);
            _dataDst.AddRange(buffer.AsArray());
            sampleTypeUrl.Dispose();
            buffer.Dispose();
        }

        [BurstDiscard]
        // ReSharper restore Unity.ExpensiveCode
        public void WriteBatch(IList<IMessage> samples)
        {
            if(samples.Count == 0)
                return;
            
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var index = 0; index < samples.Count; index++)
            {
                var sample = samples[index];
                Write(sample);
            }
        }

        [BurstDiscard]
        // ReSharper restore Unity.ExpensiveCode
        public void Write(IMessage sample)
        {
            var sampleTypeUrl = SampleTypeUrl.Alloc(sample.Descriptor, Allocator.Persistent);
            var sampleSize = sample.CalculateSize();
            var packedSampleSize = Any.ComputeSize(sampleTypeUrl.BytesLength, sampleSize);
            var buffer = new BufferWriter(packedSampleSize, Allocator.Persistent);
            Any.WriteTo(sampleTypeUrl, sample, ref buffer);
            _dataDst.AddRange(buffer.AsArray());
            sampleTypeUrl.Dispose();
            buffer.Dispose();
        }
    }
}