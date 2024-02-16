using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using PLUME.Core.Recorder.Data;
using PLUME.Core.Recorder.ProtoBurst;
using ProtoBurst.Message;
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

        void IRecorderModule.Start(RecordContext recordContext, RecorderContext recorderContext)
        {
            _isRecording = true;

            _serializationThread = new Thread(() =>
                SerializeFrameLoop(recordContext.Data, recorderContext.SampleTypeUrlRegistry))
            {
                Name = "FrameRecorderModule.SerializeThread",
                IsBackground = false
            };
            _serializationThread.Start();
        }

        void IRecorderModule.ForceStop(RecordContext recordContext, RecorderContext recorderContext)
        {
            _isRecording = false;
            _serializationThread.Join();
            _serializationThread = null;
        }

        async UniTask IRecorderModule.Stop(RecordContext recordContext, RecorderContext recorderContext)
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

        void IRecorderModule.PostLateUpdate(RecordContext recordContext, RecorderContext context)
        {
            var timestamp = recordContext.Clock.ElapsedNanoseconds;
            var frame = UnityEngine.Time.frameCount;
            EnqueueFrame(timestamp, frame);
        }

        internal void EnqueueFrame(long timestamp, int frameNumber)
        {
            var frame = new Frame(timestamp, frameNumber);

            foreach (var module in _frameDataRecorderModules)
            {
                module.PushFrameData(frame);
            }
            
            _frameQueue.Add(frame);
        }

        // TODO: use workers
        internal void SerializeFrameLoop(IRecordData data, SampleTypeUrlRegistry sampleTypeUrlRegistry)
        {
            Profiler.BeginThreadProfiling("PLUME", "FrameRecorderModule.SerializeThread");

            while (_isRecording || _frameQueue.Count > 0)
            {
                while (_frameQueue.TryTake(out var frame))
                {
                    var frameBuffer = new SerializedSamplesBuffer(Allocator.Persistent);
                    var hasData = false;

                    // ReSharper disable once ForCanBeConvertedToForeach
                    // ReSharper disable once LoopCanBeConvertedToQuery
                    for (var i = 0; i < _frameDataRecorderModules.Length; i++)
                    {
                        var module = _frameDataRecorderModules[i];
                        hasData |= module.TryPopSerializedFrameData(frame, frameBuffer);
                    }

                    if (hasData)
                    {
                        var frameSample = FrameSample.Pack(Allocator.Persistent, frame.FrameNumber,
                            ref sampleTypeUrlRegistry, ref frameBuffer);
                        var packedSample = PackedSample.Pack(Allocator.Persistent, frame.Timestamp, frameSample);
                        
                        var serializedData = new NativeList<byte>(packedSample.ComputeMaxSize(), Allocator.Persistent);
                        packedSample.WriteToNoResize(ref serializedData);
                        data.AddTimestampedDataChunk(serializedData.AsArray(), frame.Timestamp);

                        frameSample.Dispose();
                        packedSample.Dispose();
                        serializedData.Dispose();
                    }

                    frameBuffer.Dispose();
                }
            }
            
            Profiler.EndThreadProfiling();
        }
    }
}