using PLUME.Core.Recorder.ProtoBurst;
using ProtoBurst.Message;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Core.Recorder
{
    public class FrameSamplePacker
    {
        [BurstCompile]
        public void WriteFramePackedSample(long timestamp, int frameNumber,
            ref SampleTypeUrlRegistry typeUrlRegistry, ref SerializedSamplesBuffer buffer,
            ref NativeList<byte> output)
        {
            var frameSampleData = new NativeArray<Any>(buffer.ChunkCount, Allocator.Persistent);

            var offset = 0;

            var chunksData = buffer.GetData();
            var chunksLength = buffer.GetLengths();
            var chunksSampleTypeUrlIndex = buffer.GetSampleTypeUrlIndices();

            for (var chunkIdx = 0; chunkIdx < buffer.ChunkCount; chunkIdx++)
            {
                var chunkLength = chunksLength[chunkIdx];
                var chunkData = chunksData.GetSubArray(offset, chunkLength);
                var chunkSampleTypeUrlIndex = chunksSampleTypeUrlIndex[chunkIdx];
                frameSampleData[chunkIdx] =
                    Any.Pack(chunkData, typeUrlRegistry.GetTypeUrlFromIndex(chunkSampleTypeUrlIndex));
                offset += chunkLength;
            }

            var frameSample = new FrameSample(frameNumber, frameSampleData);
            var packedSample = PackedSample.Pack(Allocator.Persistent, timestamp, frameSample);
            var packedSampleMaxSize = packedSample.ComputeMaxSize();

            var serializedPackedSample = new NativeList<byte>(packedSampleMaxSize, Allocator.Persistent);
            packedSample.WriteToNoResize(ref serializedPackedSample);
            
            output.AddRange(serializedPackedSample.AsArray());

            serializedPackedSample.Dispose();
            frameSampleData.Dispose();
            packedSample.Dispose();
        }
    }
}