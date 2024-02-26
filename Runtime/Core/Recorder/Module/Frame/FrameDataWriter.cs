using System.Collections.Generic;
using Google.Protobuf;
using ProtoBurst;
using ProtoBurst.Message;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Collections;

namespace PLUME.Core.Recorder.Module.Frame
{
    public struct FrameDataWriter
    {
        private NativeList<byte> _frameDataRawBytes;

        public FrameDataWriter(NativeList<byte> frameDataRawBytes)
        {
            _frameDataRawBytes = frameDataRawBytes;
        }

        public void Write(IMessage sample)
        {
            var frameDataRawBytes = new NativeList<byte>(Allocator.Persistent);
            var bufferWriter = new BufferWriter(frameDataRawBytes);
            
            var sampleTypeUrl = SampleTypeUrl.Alloc(sample.Descriptor, Allocator.Persistent);
            var sampleTypeUrlBytes = sampleTypeUrl.AsArray();

            var sampleSize = sample.CalculateSize();
            var packedSampleSize = Any.ComputeSize(sampleTypeUrlBytes.Length, sampleSize);
            var serializedSampleSize = BufferWriterExtensions.ComputeTagSize(Sample.ProtoBurst.Frame.DataFieldTag) +
                                       BufferWriterExtensions.ComputeLengthPrefixSize(packedSampleSize) +
                                       packedSampleSize;

            frameDataRawBytes.SetCapacity(serializedSampleSize);

            bufferWriter.WriteTag(Sample.ProtoBurst.Frame.DataFieldTag);
            bufferWriter.WriteLength(serializedSampleSize);
            Any.WriteTo(ref sampleTypeUrlBytes, ref sample, ref bufferWriter);

            WriteRaw(frameDataRawBytes);

            frameDataRawBytes.Dispose();
            sampleTypeUrl.Dispose();
        }

        public void WriteBatch(IList<IMessage> samples)
        {
            if (samples.Count == 0)
                return;

            var frameDataRawBytes = new NativeList<byte>(Allocator.Persistent);
            var bufferWriter = new BufferWriter(frameDataRawBytes);
            
            var sampleTypeUrl = SampleTypeUrl.Alloc(samples[0].Descriptor, Allocator.Persistent);
            var sampleTypeUrlBytes = sampleTypeUrl.AsArray();

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var sampleIdx = 0; sampleIdx < samples.Count; sampleIdx++)
            {
                var sample = samples[sampleIdx];
                var sampleSize = sample.CalculateSize();
                var packedSampleSize = Any.ComputeSize(sampleTypeUrlBytes.Length, sampleSize);
                var serializedSampleSize = BufferWriterExtensions.ComputeTagSize(Sample.ProtoBurst.Frame.DataFieldTag) +
                                           BufferWriterExtensions.ComputeLengthPrefixSize(packedSampleSize) +
                                           packedSampleSize;

                frameDataRawBytes.SetCapacity(frameDataRawBytes.Length + serializedSampleSize);

                bufferWriter.WriteTag(Sample.ProtoBurst.Frame.DataFieldTag);
                bufferWriter.WriteLength(serializedSampleSize);
                Any.WriteTo(ref sampleTypeUrlBytes, ref sample, ref bufferWriter);
            }
            
            WriteRaw(frameDataRawBytes);

            frameDataRawBytes.Dispose();
            sampleTypeUrl.Dispose();
        }

        public void Write<TU>(TU sample) where TU : unmanaged, IProtoBurstMessage
        {
            var frameDataRawBytes = new NativeList<byte>(Allocator.Persistent);
            var bufferWriter = new BufferWriter(frameDataRawBytes);
            
            var sampleTypeUrl = sample.GetTypeUrl(Allocator.Persistent);
            var sampleTypeUrlBytes = sampleTypeUrl.AsArray();

            var sampleSize = sample.ComputeSize();
            var packedSampleSize = Any.ComputeSize(sampleTypeUrlBytes.Length, sampleSize);
            var serializedSampleSize = BufferWriterExtensions.ComputeTagSize(Sample.ProtoBurst.Frame.DataFieldTag) +
                                       BufferWriterExtensions.ComputeLengthPrefixSize(packedSampleSize) +
                                       packedSampleSize;

            frameDataRawBytes.SetCapacity(serializedSampleSize);

            bufferWriter.WriteTag(Sample.ProtoBurst.Frame.DataFieldTag);
            bufferWriter.WriteLength(serializedSampleSize);
            Any.WriteTo(ref sampleTypeUrlBytes, ref sample, ref bufferWriter);

            WriteRaw(frameDataRawBytes);

            frameDataRawBytes.Dispose();
            sampleTypeUrl.Dispose();
        }

        public void WriteBatch<TU, TV>(NativeArray<TU> samples, TV batchSerializer, int batchSize = 128)
            where TU : unmanaged, IProtoBurstMessage
            where TV : struct, IFrameDataBatchSerializer<TU>
        {
            if (samples.Length == 0)
                return;

            var frameDataRawBytes = batchSerializer.SerializeFrameDataBatch(samples, Allocator.Persistent, batchSize);
            WriteRaw(frameDataRawBytes);
            frameDataRawBytes.Dispose();
        }

        public void WriteRaw(NativeList<byte> frameDataRawBytes)
        {
            _frameDataRawBytes.AddRange(frameDataRawBytes.AsArray());
        }
    }
}