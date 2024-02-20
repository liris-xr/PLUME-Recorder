using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using PLUME.Core.Recorder.Data;
using PLUME.Core.Recorder.ProtoBurst;
using ProtoBurst;
using ProtoBurst.Message;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Scripting;

namespace PLUME.Core.Recorder.Module
{
    /// <summary>
    /// The frame recorder is responsible for recording data associated with Unity frames.
    /// </summary>
    [Preserve]
    public class FrameRecorderModule : IRecorderModule
    {
        private bool _isRecording;

        private IFrameDataRecorderModule[] _frameDataRecorderModules;

        private Thread _serializationThread;
        private BlockingCollection<Frame> _frameQueue;

        private SampleTypeUrl _frameSampleTypeUrl;

        void IRecorderModule.Create(RecorderContext recorderContext)
        {
            _frameQueue = new BlockingCollection<Frame>(new ConcurrentQueue<Frame>());
            _frameDataRecorderModules = recorderContext.Modules.OfType<IFrameDataRecorderModule>().ToArray();
            _frameSampleTypeUrl = SampleTypeUrl.Alloc(FrameSample.TypeUrl, Allocator.Persistent);
        }

        void IRecorderModule.Destroy(RecorderContext recorderContext)
        {
            _frameSampleTypeUrl.Dispose();
            _frameQueue.Dispose();
        }

        void IRecorderModule.StartRecording(Record record, RecorderContext recorderContext)
        {
            _isRecording = true;

            _serializationThread = new Thread(() => SerializeFrameLoop(record))
            {
                Name = "FrameRecorderModule.SerializeThread",
                IsBackground = false
            };
            _serializationThread.Start();
        }

        void IRecorderModule.ForceStopRecording(Record record, RecorderContext recorderContext)
        {
            _isRecording = false;
            var remainingFramesCount = _frameQueue.Count;
            if (remainingFramesCount > 0)
                Logger.Log(nameof(FrameRecorderModule), $"{remainingFramesCount} frames left in queue.");
            _serializationThread.Join();
            _serializationThread = null;
        }

        async UniTask IRecorderModule.StopRecording(Record record, RecorderContext recorderContext)
        {
            _isRecording = false;
            await UniTask.WaitUntil(() => !_serializationThread.IsAlive);
            _serializationThread = null;
        }

        void IRecorderModule.Reset(RecorderContext context)
        {
        }

        void IRecorderModule.PostLateUpdate(Record record, RecorderContext context)
        {
            var timestamp = record.Clock.ElapsedNanoseconds;
            var frame = UnityEngine.Time.frameCount;
            PushFrame(timestamp, frame);
        }

        private void PushFrame(long timestamp, int frameNumber)
        {
            var frame = new Frame(timestamp, frameNumber);

            foreach (var module in _frameDataRecorderModules)
            {
                module.CollectFrameData(frame);
            }

            _frameQueue.Add(frame);
        }

        // TODO: use workers
        internal void SerializeFrameLoop(Record record)
        {
            Profiler.BeginThreadProfiling("PLUME", "FrameRecorderModule.SerializeThread");

            while (_isRecording || _frameQueue.Count > 0)
            {
                while (_frameQueue.TryTake(out var frame))
                {
                    var hasData = false;
                    
                    var serializedSamplesData = new DataChunks(Allocator.Persistent);
                    var serializedSamplesTypeUrl = new DataChunks(Allocator.Persistent);
                    
                    var frameDataWriter = new FrameDataWriter(serializedSamplesData, serializedSamplesTypeUrl);
                    
                    foreach (var module in _frameDataRecorderModules)
                    {
                        hasData |= module.SerializeFrameData(frame, frameDataWriter);
                        module.DisposeFrameData(frame);
                    }
                    
                    if (hasData)
                    {
                        Profiler.BeginSample("Pack frame");
                        var frameSample = new FrameSample(frame.FrameNumber, serializedSamplesData,
                            serializedSamplesTypeUrl);
                        
                        var frameSampleBytes = SerializeFrameSampleParallel(ref frameSample, Allocator.Persistent);
                        var frameSampleTypeUrlSpan = _frameSampleTypeUrl.AsReadOnlySpan();
                        var frameSampleBytesSpan = frameSampleBytes.AsArray().AsReadOnlySpan();
                        var payload = Any.Pack(frameSampleBytesSpan, frameSampleTypeUrlSpan, Allocator.Persistent);
                        frameSampleBytes.Dispose();
                        
                        var packedSample = new PackedSample(frame.Timestamp, payload);
                        record.RecordTimestampedPackedSample(packedSample, frame.Timestamp);
                        payload.Dispose();
                        Profiler.EndSample();
                    }
                    
                    serializedSamplesData.Dispose();
                    serializedSamplesTypeUrl.Dispose();
                }
            }

            Profiler.EndThreadProfiling();
        }

        private static NativeList<byte> SerializeFrameSampleParallel(ref FrameSample sample, Allocator allocator,
            int batchSize = 128)
        {
            var initialSize = BufferExtensions.ComputeTagSize(FrameSample.FrameNumberFieldTag) +
                              BufferExtensions.ComputeInt32Size(sample.FrameNumber);

            var frameDataBytes = new NativeList<byte>(initialSize, allocator);
            var bufferWriter = new BufferWriter(frameDataBytes);
            bufferWriter.WriteTag(FrameSample.FrameNumberFieldTag);
            bufferWriter.WriteInt32(sample.FrameNumber);
            
            var nChunks = sample.SerializedSamplesData.ChunksCount;
            var chunksSize = new NativeArray<int>(nChunks, allocator);
            var chunksTotalSize = new NativeArray<int>(1, allocator);
            var dataChunksByteOffset = new NativeArray<int>(nChunks, allocator);
            var typeUrlChunksByteOffset = new NativeArray<int>(nChunks, allocator);
            
            new PrepareWriteDataParallelJob
            {
                FrameSample = sample,
                ChunksSize = chunksSize,
                ChunksTotalSize = chunksTotalSize,
                DataChunksByteOffset = dataChunksByteOffset,
                TypeUrlChunksByteOffset = typeUrlChunksByteOffset
            }.Run(nChunks, batchSize);
            
            frameDataBytes.Capacity += chunksTotalSize[0];
            chunksTotalSize.Dispose();
            
            new WriteDataParallelJob
            {
                FrameDataBytes = frameDataBytes.AsParallelWriter(),
                FrameSample = sample,
                ChunksSize = chunksSize,
                DataChunksByteOffset = dataChunksByteOffset,
                TypeUrlChunksByteOffset = typeUrlChunksByteOffset
            }.Run(nChunks, batchSize);
            
            chunksSize.Dispose();
            dataChunksByteOffset.Dispose();
            typeUrlChunksByteOffset.Dispose();
            return frameDataBytes;
        }

        [BurstCompile]
        private struct PrepareWriteDataParallelJob : IJobParallelForBatch
        {
            [ReadOnly]
            public FrameSample FrameSample;

            public NativeArray<int> ChunksTotalSize;
            [WriteOnly] public NativeArray<int> ChunksSize;
            [WriteOnly] public NativeArray<int> DataChunksByteOffset;
            [WriteOnly] public NativeArray<int> TypeUrlChunksByteOffset;

            public unsafe void Execute(int startIndex, int count)
            {
                var dataByteOffset = 0;
                var typeUrlByteOffset = 0;

                var chunkSizes = 0;

                for (var chunkIdx = startIndex; chunkIdx < startIndex + count; chunkIdx++)
                {
                    var typeUrlBytesLength = FrameSample.SerializedSamplesTypeUrl.GetLength(chunkIdx);
                    var dataBytesLength = FrameSample.SerializedSamplesData.GetLength(chunkIdx);
                    
                    TypeUrlChunksByteOffset[chunkIdx] = typeUrlByteOffset;
                    DataChunksByteOffset[chunkIdx] = dataByteOffset;
                    
                    var chunkSize = FrameSample.ComputeDataChunkSize(typeUrlBytesLength, dataBytesLength);
                    ChunksSize[chunkIdx] = chunkSize;
                    chunkSizes += chunkSize;
                    typeUrlByteOffset += typeUrlBytesLength;
                    dataByteOffset += dataBytesLength;
                }

                Interlocked.Add(ref ((int*)ChunksTotalSize.GetUnsafePtr())[0], chunkSizes);
            }
        }

        [BurstCompile]
        private struct WriteDataParallelJob : IJobParallelForBatch
        {
            public NativeList<byte>.ParallelWriter FrameDataBytes;

            [NativeDisableParallelForRestriction] public FrameSample FrameSample;

            [ReadOnly] public NativeArray<int> ChunksSize;
            [ReadOnly] public NativeArray<int> DataChunksByteOffset;
            [ReadOnly] public NativeArray<int> TypeUrlChunksByteOffset;

            public void Execute(int startIndex, int count)
            {
                var serializedSampleData = FrameSample.SerializedSamplesData.GetDataSpan();
                var serializedSampleTypeUrl = FrameSample.SerializedSamplesTypeUrl.GetDataSpan();

                // ReSharper disable once ForCanBeConvertedToForeach
                for (var chunkIdx = startIndex; chunkIdx < startIndex + count; chunkIdx++)
                {
                    var typeUrlBytesLength = FrameSample.SerializedSamplesTypeUrl.GetLength(chunkIdx);
                    var valueBytesLength = FrameSample.SerializedSamplesData.GetLength(chunkIdx);
                    
                    var typeUrlByteOffset = TypeUrlChunksByteOffset[chunkIdx];
                    var valueByteOffset = DataChunksByteOffset[chunkIdx];
                    
                    var typeUrlBytes = serializedSampleTypeUrl.Slice(typeUrlByteOffset, typeUrlBytesLength);
                    var valueBytes = serializedSampleData.Slice(valueByteOffset, valueBytesLength);

                    var chunkSize = ChunksSize[chunkIdx];
                    
                    var bytes = new NativeList<byte>(chunkSize, Allocator.Temp);
                    var bufferWriter = new BufferWriter(bytes);
                    
                    unsafe
                    {
                        fixed (byte* valueBytesPtr = valueBytes)
                        {
                            fixed (byte* typeUrlBytesPtr = typeUrlBytes)
                            {
                                FrameSample.WriteDataChunkTo(typeUrlBytesPtr, typeUrlBytesLength, valueBytesPtr,
                                    valueBytesLength,
                                    ref bufferWriter);
                            }
                        }
                    }
                    
                    FrameDataBytes.AddRangeNoResize(bytes);
                    bytes.Dispose();
                }
            }
        }
    }
}