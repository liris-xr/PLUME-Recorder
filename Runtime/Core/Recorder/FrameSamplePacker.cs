using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using PLUME.Core.Recorder.Data;
using PLUME.Core.Recorder.ProtoBurst;
using ProtoBurst.Message;
using Unity.Collections;
using UnityEngine.Profiling;

namespace PLUME.Core.Recorder
{
    public class FrameSamplePacker
    {
        public async UniTask WriteFramePackedSampleAsync(long timestamp, int frameNumber,
            SampleTypeUrlRegistry typeUrlRegistry, SerializedSamplesBuffer buffer,
            IRecordData output)
        {
            var frameDataSamples = new NativeArray<Any>(buffer.ChunkCount, Allocator.Persistent);

            await UniTask.SwitchToThreadPool();

            Profiler.BeginSample("WriteFramePackedSampleAsync.1");
            var offset = 0;

            var chunksData = buffer.GetData();
            var chunksLength = buffer.GetLengths();
            var chunksSampleTypeUrlIndex = buffer.GetSampleTypeUrlIndices();

            for (var chunkIdx = 0; chunkIdx < buffer.ChunkCount; chunkIdx++)
            {
                var chunkLength = chunksLength[chunkIdx];
                var chunkData = chunksData.GetSubArray(offset, chunkLength);
                var chunkSampleTypeUrlIndex = chunksSampleTypeUrlIndex[chunkIdx];
                frameDataSamples[chunkIdx] =
                    Any.Pack(chunkData, typeUrlRegistry.GetTypeUrlFromIndex(chunkSampleTypeUrlIndex));
                offset += chunkLength;
            }

            Profiler.EndSample();

            Profiler.BeginSample("WriteFramePackedSampleAsync.1.bis");
            var frameSample = new FrameSample(frameNumber, frameDataSamples);
            var frameSampleMaxSize = frameSample.ComputeMaxSize();
            Profiler.EndSample();

            await UniTask.SwitchToMainThread();

            var frameSampleData = new NativeList<byte>(frameSampleMaxSize, Allocator.Persistent);

            await UniTask.SwitchToThreadPool();

            Profiler.BeginSample("WriteFramePackedSampleAsync.2");
            frameSample.WriteToNoResize(ref frameSampleData);
            var packedSample = PackedSample.Pack(timestamp, frameSampleData.AsArray(), FrameSample.FrameSampleTypeUrl);
            var packedSampleMaxSize = packedSample.ComputeMaxSize();
            Profiler.EndSample();

            await UniTask.SwitchToMainThread();

            var data = new NativeList<byte>(packedSampleMaxSize, Allocator.Persistent);

            await UniTask.SwitchToThreadPool();

            Profiler.BeginSample("WriteFramePackedSampleAsync.3");
            packedSample.WriteToNoResize(ref data);
            Profiler.EndSample();

            await UniTask.SwitchToMainThread();

            Profiler.BeginSample("WriteFramePackedSampleAsync.4");
            output.AddTimestampedData(data.AsArray(), timestamp);
            Profiler.EndSample();

            data.Dispose();
            frameDataSamples.Dispose();
            frameSampleData.Dispose();
        }
    }
}