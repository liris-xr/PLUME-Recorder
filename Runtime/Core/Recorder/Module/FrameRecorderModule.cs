using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using PLUME.Core.Recorder.Data;
using Unity.Collections;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Profiling;
using UnityEngine.Scripting;

namespace PLUME.Core.Recorder.Module
{
    /// <summary>
    /// The frame recorder is responsible for recording data associated with Unity frames.
    /// It automatically runs after <see cref="PostLateUpdate"/> if <see cref="InjectUpdateInCurrentLoop"/> was called (automatically called by <see cref="Recorder"/> when the instance is created).
    /// It is responsible for running the <see cref="IFrameDataRecorderModule.RecordFrameData"/> and <see cref="IFrameDataRecorderModule.RecordFrameData"/> on <see cref="IFrameDataRecorderModule"/> and <see cref="IFrameDataRecorderModule"/> modules respectively.
    /// </summary>
    [Preserve]
    public class FrameRecorderModule : IRecorderModule
    {
        // TODO: convert to a utility class?
        private FrameSamplePacker _frameSamplePacker;

        private IFrameDataRecorderModule[] _frameDataRecorderModules;

        private struct FrameInfo
        {
            public long Timestamp;
            public int FrameNumber;
        }

        private readonly BlockingCollection<FrameInfo> _frameInfoQueue = new(new ConcurrentQueue<FrameInfo>());
        private Thread _frameSerializationThread;

        private bool _shouldSerialize;

        void IRecorderModule.Create(RecorderContext recorderContext)
        {
            _frameSamplePacker = new FrameSamplePacker();
            _frameDataRecorderModules = recorderContext.Modules.OfType<IFrameDataRecorderModule>().ToArray();
        }

        void IRecorderModule.Start(RecordContext recordContext, RecorderContext recorderContext)
        {
            _shouldSerialize = true;

            _frameSerializationThread = new Thread(() => SerializeFrameLoop(recordContext.Data, recorderContext.SampleTypeUrlRegistry))
            {
                Name = "FrameRecorderModule.SerializeThread",
                IsBackground = false
            };
            _frameSerializationThread.Start();
        }

        void IRecorderModule.ForceStop(RecordContext recordContext, RecorderContext recorderContext)
        {
            _shouldSerialize = false;
            _frameSerializationThread.Join();
        }
        
        async UniTask IRecorderModule.Stop(RecordContext recordContext, RecorderContext recorderContext)
        {
            _shouldSerialize = false;
            await UniTask.WaitUntil(() => !_frameSerializationThread.IsAlive);
            _frameSerializationThread = null;
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
            foreach (var module in _frameDataRecorderModules)
            {
                module.EnqueueFrameData();
            }

            _frameInfoQueue.Add(new FrameInfo
            {
                Timestamp = timestamp,
                FrameNumber = frameNumber
            });
        }

        internal void SerializeFrameLoop(IRecordData data, SampleTypeUrlRegistry sampleTypeUrlRegistry)
        {
            Profiler.BeginThreadProfiling("PLUME", "FrameRecorderModule.SerializeThread");
            
            while (_shouldSerialize || _frameInfoQueue.Count > 0)
            {
                while (_frameInfoQueue.TryTake(out var frameInfo))
                {
                    var frameBuffer = new SerializedSamplesBuffer(Allocator.Persistent);

                    foreach (var recorderModule in _frameDataRecorderModules)
                    {
                        recorderModule.DequeueSerializedFrameData(frameBuffer);
                    }

                    var serializedData = new NativeList<byte>(Allocator.Persistent);
                    
                    _frameSamplePacker.WriteFramePackedSample(frameInfo.Timestamp, frameInfo.FrameNumber, ref sampleTypeUrlRegistry, ref frameBuffer, ref serializedData);
                    
                    data.AddTimestampedDataChunk(serializedData.AsArray(), frameInfo.Timestamp);
                    serializedData.Dispose();
                    frameBuffer.Dispose();
                }
            }
            
            Profiler.EndThreadProfiling();
        }

        public int GetRemainingTasksCount()
        {
            return _frameInfoQueue.Count;
        }
    }
}