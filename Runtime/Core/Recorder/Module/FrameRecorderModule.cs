using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using PLUME.Core.Recorder.Data;
using PLUME.Core.Recorder.ProtoBurst;
using Unity.Collections;
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

        void IRecorderModule.Create(RecorderContext recorderContext)
        {
            _frameQueue = new BlockingCollection<Frame>(new ConcurrentQueue<Frame>());
            _frameDataRecorderModules = recorderContext.Modules.OfType<IFrameDataRecorderModule>().ToArray();
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

        void IRecorderModule.Destroy(RecorderContext recorderContext)
        {
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

            ulong totalTimeSum = 0;
            ulong serializationTimeSum = 0;
            var frameCounter = 0;
            
            var serializedWhileRecording = 0;
            var serializedAfterRecording = 0;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var stopwatch2 = System.Diagnostics.Stopwatch.StartNew();
            
            while (_isRecording || _frameQueue.Count > 0)
            {
                while (_frameQueue.TryTake(out var frame))
                {
                    if(_isRecording)
                        serializedWhileRecording++;
                    else
                        serializedAfterRecording++;
                    
                    stopwatch.Restart();

                    var hasData = false;

                    var frameDataChunks = new DataChunks(Allocator.Persistent);
                    var frameDataWriter = new FrameDataWriter(frameDataChunks);

                    stopwatch2.Restart();
                    foreach (var module in _frameDataRecorderModules)
                    {
                        hasData |= module.SerializeFrameData(frame, frameDataWriter);
                        module.DisposeFrameData(frame);
                    }
                    stopwatch2.Stop();

                    if (hasData)
                    {
                        var frameData = frameDataChunks.GetChunksData();
                        var frameSample = FrameSample.Pack(frame.FrameNumber, frameData, Allocator.Persistent);
                        record.RecordTimestampedSample(frameSample, frame.Timestamp);
                        frameSample.Dispose();
                    }

                    frameDataChunks.Dispose();
                    stopwatch.Stop();
                    totalTimeSum += (ulong)stopwatch.ElapsedMilliseconds;
                    serializationTimeSum += (ulong)stopwatch2.ElapsedMilliseconds;
                    frameCounter++;
                }
            }
            
            Logger.Log(nameof(FrameRecorderModule), $"Average frame total serialization time: {totalTimeSum / (float) frameCounter}ms");
            Logger.Log(nameof(FrameRecorderModule), $"Average frame serialization time: {serializationTimeSum / (float) frameCounter}ms");
            Logger.Log(nameof(FrameRecorderModule), $"Serialized while recording: {serializedWhileRecording}");
            Logger.Log(nameof(FrameRecorderModule), $"Serialized after recording: {serializedAfterRecording}");
            
            Profiler.EndThreadProfiling();
        }
    }
}