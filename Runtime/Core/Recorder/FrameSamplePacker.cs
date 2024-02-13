using PLUME.Core.Recorder.ProtoBurst;
using ProtoBurst;
using ProtoBurst.Message;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace PLUME.Core.Recorder
{
    [BurstCompile]
    public class FrameSamplePacker
    {
        [BurstDiscard]
        public static JobHandle WriteFramePackedSampleAsync(long timestamp, int frameNumber,
            SampleTypeUrlRegistry typeUrlRegistry, SerializedSamplesBuffer buffer, NativeList<byte> data,
            JobHandle dependsOn = default)
        {
            var job = new AsyncWritingJob
            {
                Timestamp = timestamp,
                FrameNumber = frameNumber,
                Buffer = buffer,
                Data = data,
                TypeUrlRegistry = typeUrlRegistry
            };
            return job.Schedule(dependsOn);
        }

        [BurstCompile]
        public static void WriteFramePackedSample(long timestamp, int frameNumber, ref SerializedSamplesBuffer buffer,
            ref NativeList<byte> data, ref SampleTypeUrlRegistry typeUrlRegistry)
        {
            var offset = 0;
            var frameData = new NativeArray<Any>(buffer.ChunkCount, Allocator.Temp);

            for (var chunkIdx = 0; chunkIdx < buffer.ChunkCount; chunkIdx++)
            {
                var chunkLength = buffer.GetLengths()[chunkIdx];
                var chunkData = buffer.GetData(offset, chunkLength);
                var chunkSampleTypeUrlIndex = buffer.GetSampleTypeUrlIndices()[chunkIdx];
                var msgTypeUrl = typeUrlRegistry.GetTypeUrlFromIndex(chunkSampleTypeUrlIndex);
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

        [BurstCompile]
        private struct AsyncWritingJob : IJob
        {
            public long Timestamp;
            public int FrameNumber;
            [ReadOnly] public SerializedSamplesBuffer Buffer;
            public NativeList<byte> Data;
            [ReadOnly] public SampleTypeUrlRegistry TypeUrlRegistry;

            public void Execute()
            {
                WriteFramePackedSample(Timestamp, FrameNumber, ref Buffer, ref Data, ref TypeUrlRegistry);
            }
        }
    }
}