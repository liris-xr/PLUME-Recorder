using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using PLUME.Sample.ProtoBurst;
using ProtoBurst;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Scripting;

namespace PLUME.Core.Recorder.Module.Frame
{
    /// <summary>
    /// The frame recorder is responsible for recording data associated with Unity frames.
    /// </summary>
    [Preserve]
    public class FrameRecorderModule : IRecorderModule
    {
        public bool IsRecording { get; private set; }

        private IFrameDataRecorderModule[] _frameDataRecorderModules;

        private Thread _serializationThread;
        private BlockingCollection<FrameInfo> _frameQueue;

        void IRecorderModule.Create(RecorderContext recorderContext)
        {
            _frameQueue = new BlockingCollection<FrameInfo>(new ConcurrentQueue<FrameInfo>());
            _frameDataRecorderModules = recorderContext.Modules.OfType<IFrameDataRecorderModule>().ToArray();
        }

        void IRecorderModule.Destroy(RecorderContext recorderContext)
        {
            _frameQueue.Dispose();
        }

        void IRecorderModule.Awake(RecorderContext context)
        {
            
        }

        void IRecorderModule.StartRecording(Record record, RecorderContext recorderContext)
        {
            IsRecording = true;

            _serializationThread = new Thread(() => SerializeFrameLoop(record))
            {
                Name = "FrameRecorderModule.SerializeThread",
                IsBackground = false
            };
            _serializationThread.Start();
        }

        void IRecorderModule.ForceStopRecording(Record record, RecorderContext recorderContext)
        {
            IsRecording = false;
            var remainingFramesCount = _frameQueue.Count;
            if (remainingFramesCount > 0)
                Logger.Log(nameof(FrameRecorderModule), $"{remainingFramesCount} frames left in queue.");
            _serializationThread.Join();
            _serializationThread = null;
        }

        async UniTask IRecorderModule.StopRecording(Record record, RecorderContext recorderContext)
        {
            IsRecording = false;
            await UniTask.WaitUntil(() => !_serializationThread.IsAlive);
            _serializationThread = null;
        }

        void IRecorderModule.PostLateUpdate(Record record, RecorderContext context)
        {
            var timestamp = record.Clock.ElapsedNanoseconds;
            var frame = Time.frameCount;
            PushFrame(timestamp, frame);
        }

        private void PushFrame(long timestamp, int frameNumber)
        {
            var frame = new FrameInfo(timestamp, frameNumber);

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

            // This buffer is reused for each frame.
            var frameDataRawBytes = new NativeList<byte>(Allocator.Persistent);
            var frameDataWriter = new FrameDataWriter(frameDataRawBytes);

            var frameSampleTypeUrl = SampleTypeUrl.Alloc(FrameSample.TypeUrl, Allocator.Persistent);

            while (IsRecording || _frameQueue.Count > 0)
            {
                while (_frameQueue.TryTake(out var frame))
                {
                    frameDataRawBytes.Clear();

                    var hasData = false;

                    foreach (var module in _frameDataRecorderModules)
                    {
                        hasData |= module.SerializeFrameData(frame, frameDataWriter);
                        module.DisposeFrameData(frame);
                    }

                    if (!hasData) continue;

                    var frameSample = FrameSample.Pack(frame.FrameNumber, ref frameDataRawBytes, Allocator.Persistent);
                    var frameSampleBytes = frameSample.ToBytes(Allocator.Persistent);
                    frameSample.Dispose();

                    record.RecordTimestampedSample(frameSampleBytes, frameSampleTypeUrl, frame.Timestamp);
                    frameSampleBytes.Dispose();
                }
            }

            frameSampleTypeUrl.Dispose();
            frameDataRawBytes.Dispose();
            Profiler.EndThreadProfiling();
        }
    }
}