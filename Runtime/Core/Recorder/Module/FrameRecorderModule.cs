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
                    
                    var frameDataRawBytes = new NativeList<byte>(Allocator.Persistent);
                    var frameDataWriter = new FrameDataWriter(frameDataRawBytes);
                    
                    foreach (var module in _frameDataRecorderModules)
                    {
                        hasData |= module.SerializeFrameData(frame, frameDataWriter);
                        module.DisposeFrameData(frame);
                    }
                    
                    if (hasData)
                    {
                        Profiler.BeginSample("Pack frame");
                        
                        var frameSample = new FrameSample(frame.FrameNumber, frameDataRawBytes);
                        var payload = Any.Pack(frameSample, Allocator.Persistent);
                        var packedSample = new PackedSample(frame.Timestamp, payload);
                        record.RecordTimestampedPackedSample(packedSample, frame.Timestamp);
                        payload.Dispose();
                        
                        Profiler.EndSample();
                    }
                    
                    frameDataRawBytes.Dispose();
                }
            }

            Profiler.EndThreadProfiling();
        }
    }
}