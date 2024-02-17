using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using PLUME.Core.Recorder.Data;
using PLUME.Core.Recorder.ProtoBurst;
using Unity.Collections;
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

            _serializationThread = new Thread(() => SerializeFrameLoop(record.DataBuffer, recorderContext.SampleTypeUrlRegistry))
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
        internal void SerializeFrameLoop(RecordDataBuffer dataBuffer, SampleTypeUrlRegistry sampleTypeUrlRegistry)
        {
            Profiler.BeginThreadProfiling("PLUME", "FrameRecorderModule.SerializeThread");

            while (_isRecording || _frameQueue.Count > 0)
            {
                while (_frameQueue.TryTake(out var frame))
                {
                    var nRecorderModules = _frameDataRecorderModules.Length;

                    var hasData = false;
                    
                    var frameDataChunks = new FrameDataChunks(Allocator.Persistent);

                    for (var i = 0; i < nRecorderModules; i++)
                    {
                        var module = _frameDataRecorderModules[i];
                        hasData |= module.SerializeFrameData(frame, frameDataChunks);
                        module.DisposeFrameData(frame);
                    }

                    if (hasData)
                    {
                        var frameSample = FrameSample.Pack(frame.FrameNumber,
                            ref sampleTypeUrlRegistry, ref frameDataChunks, Allocator.Persistent);
                        dataBuffer.AddTimestampedSample(frameSample, frame.Timestamp);
                        frameSample.Dispose();
                    }

                    frameDataChunks.Dispose();
                }
            }

            Profiler.EndThreadProfiling();
        }
    }
}