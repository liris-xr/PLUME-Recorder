using PLUME.Core.Recorder.ProtoBurst;
using ProtoBurst;
using ProtoBurst.Message;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Core.Recorder
{
    [BurstCompile]
    public class FrameSamplePacker
    {
        [BurstCompile]
        public void WritePackedFrameSample(long timestamp, int frameNumber, ref SerializedSamplesBuffer buffer,
            ref NativeList<byte> data)
        {
            var offset = 0;
            var frameData = new NativeArray<Any>(buffer.ChunkCount, Allocator.Temp);

            for (var chunkIdx = 0; chunkIdx < buffer.ChunkCount; chunkIdx++)
            {
                var chunkLength = buffer.GetLengths()[chunkIdx];
                var chunkData = buffer.GetData(offset, chunkLength);
                var chunkSampleTypeUrlIndex = buffer.GetSampleTypeUrlIndices()[chunkIdx];

                var msgTypeUrl = SampleTypeUrlRegistry.Instance.GetTypeUrlFromIndex(chunkSampleTypeUrlIndex);
                frameData[chunkIdx] = Any.Pack(chunkData, msgTypeUrl);
                offset += chunkLength;
            }

            var frame = new Frame(frameNumber, frameData);
            var packedSample = PackedSample.Pack(Allocator.Temp, timestamp, frame);
            WritingPrimitives.WriteMessage(ref packedSample, ref data);
            packedSample.Dispose();

            foreach (var fd in frameData)
            {
                fd.Dispose();
            }
        }
    }
}