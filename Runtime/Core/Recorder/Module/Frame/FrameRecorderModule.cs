using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using PLUME.Core.Settings;
using PLUME.Core.Utils;
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
    internal sealed class FrameRecorderModule : IRecorderModule
    {
        public bool IsRecording { get; private set; }

        private IFrameDataRecorderModule[] _frameDataRecorderModules;

        private Thread _serializationThread;
        private bool _shouldSerialize;

        private BlockingCollection<FrameInfo> _frameQueue;

        private Record _record;
        private RecorderContext _context;

        private long _updateInterval; // in nanoseconds

        private long _lastUpdateTime; // in nanoseconds
        private long _lastFixedUpdateTime; // in nanoseconds
        private long _deltaTime; // in nanoseconds
        private bool _shouldRunUpdate;

        void IRecorderModule.Create(RecorderContext recorderContext)
        {
            _context = recorderContext;
            _frameQueue = new BlockingCollection<FrameInfo>(new ConcurrentQueue<FrameInfo>());
            _frameDataRecorderModules = recorderContext.Modules.OfType<IFrameDataRecorderModule>().ToArray();

            var settings = FrameRecorderModuleSettings.GetOrCreate();
            _updateInterval = (long)(1_000_000_000 / settings.UpdateRate);

            PlayerLoopUtils.InjectEarlyUpdate<RecorderEarlyUpdate>(EarlyUpdate);
            PlayerLoopUtils.InjectPreUpdate<RecorderPreUpdate>(PreUpdate);
            PlayerLoopUtils.InjectUpdate<RecorderUpdate>(Update);
            PlayerLoopUtils.InjectPreLateUpdate<RecorderPreLateUpdate>(PreLateUpdate);
            PlayerLoopUtils.InjectPostLateUpdate<RecorderPostLateUpdate>(PostLateUpdate);
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
            _record = record;
            IsRecording = true;
            _lastUpdateTime = 0;
            _shouldRunUpdate = true;
            _shouldSerialize = true;

            _serializationThread = new Thread(() => SerializeFrameLoop(record))
            {
                Name = "FrameRecorderModule.SerializeThread",
                IsBackground = false
            };

            _serializationThread.Start();
        }

        void IRecorderModule.StopRecording(Record record, RecorderContext recorderContext)
        {
            IsRecording = false;
            _shouldRunUpdate = false;
            _shouldSerialize = false;
            _serializationThread.Join();
            _serializationThread = null;
            _record = null;
        }

        internal async UniTask CompleteSerializationAsync()
        {
            _shouldSerialize = false;

            var remainingFramesCount = _frameQueue.Count;
            if (remainingFramesCount > 0)
                Logger.Log(nameof(FrameRecorderModule), $"{remainingFramesCount} frames left in queue.");

            await UniTask.WaitUntil(() => !_serializationThread.IsAlive);
        }

        private void EarlyUpdate()
        {
            UpdateShouldRunUpdateFlag();
            RunFixedUpdate();

            if (!_shouldRunUpdate || !IsRecording)
                return;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _frameDataRecorderModules.Length; i++)
            {
                _frameDataRecorderModules[i].EarlyUpdate(_deltaTime, _record, _context);
            }
        }

        private void RunFixedUpdate()
        {
            if (_updateInterval == 0 || !IsRecording)
                return;
            
            var time = _record.Time;
            var fixedUpdateDt = time - _lastFixedUpdateTime;
            
            if (fixedUpdateDt < _updateInterval)
                return;
            
            var nFixedUpdates = fixedUpdateDt / _updateInterval;
            var fixedTime = _lastFixedUpdateTime;

            _record.FixedTime = fixedTime;

            for (var i = 0; i < nFixedUpdates; i++)
            {
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var j = 0; j < _frameDataRecorderModules.Length; j++)
                {
                    _frameDataRecorderModules[j].FixedUpdate(_updateInterval, _record, _context);
                }

                fixedTime += _updateInterval;
                _record.FixedTime = fixedTime;
            }

            _lastFixedUpdateTime = fixedTime;
        }

        private void UpdateShouldRunUpdateFlag()
        {
            if (_updateInterval == 0 || !IsRecording)
            {
                _shouldRunUpdate = false;
                return;
            }

            var time = _record.Time;
            var updateDt = time - _lastUpdateTime;
            var nextUpdateDt = time + Time.unscaledTimeAsDouble * 1_000_000_000 - _lastUpdateTime;

            // If the next frame is closer to the update interval than the current frame, wait for next frame
            if (Math.Abs(nextUpdateDt - _updateInterval) < Math.Abs(updateDt - _updateInterval))
            {
                _shouldRunUpdate = false;
                return;
            }

            // If the next frame is closer to the update interval than the current frame, wait for next frame
            if (updateDt > _updateInterval)
            {
                _deltaTime = updateDt;
                _lastUpdateTime = time;
                _shouldRunUpdate = true;
                return;
            }

            _shouldRunUpdate = false;
        }

        private void PreUpdate()
        {
            if (!_shouldRunUpdate || !IsRecording)
                return;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _frameDataRecorderModules.Length; i++)
            {
                _frameDataRecorderModules[i].PreUpdate(_deltaTime, _record, _context);
            }
        }

        private void Update()
        {
            if (!_shouldRunUpdate || !IsRecording)
                return;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _frameDataRecorderModules.Length; i++)
            {
                _frameDataRecorderModules[i].Update(_deltaTime, _record, _context);
            }
        }

        private void PreLateUpdate()
        {
            if (!_shouldRunUpdate || !IsRecording)
                return;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _frameDataRecorderModules.Length; i++)
            {
                _frameDataRecorderModules[i].PreLateUpdate(_deltaTime, _record, _context);
            }
        }

        private void PostLateUpdate()
        {
            if (!_shouldRunUpdate || !IsRecording)
                return;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _frameDataRecorderModules.Length; i++)
            {
                _frameDataRecorderModules[i].PostLateUpdate(_deltaTime, _record, _context);
            }

            var timestamp = _record.Time;
            var frame = Time.frameCount;
            PushFrame(timestamp, frame);
        }

        private void PushFrame(long timestamp, int frameNumber)
        {
            var frame = new FrameInfo(timestamp, frameNumber);

            foreach (var module in _frameDataRecorderModules)
            {
                module.EnqueueFrameData(frame, _record, _context);
            }
            
            _frameQueue.Add(frame);
            
            foreach (var module in _frameDataRecorderModules)
            {
                module.PostEnqueueFrameData(_record, _context);
            }
        }

        internal void SerializeFrameLoop(Record record)
        {
            Profiler.BeginThreadProfiling("PLUME", "FrameRecorderModule.SerializeThread");

            // This buffer is reused for each frame.
            var frameDataRawBytes = new NativeList<byte>(Allocator.Persistent);
            var frameDataWriter = new FrameDataWriter(frameDataRawBytes);

            var frameSampleTypeUrl = SampleTypeUrl.Alloc(Sample.ProtoBurst.Frame.TypeUrl, Allocator.Persistent);

            while (_shouldSerialize || _frameQueue.Count > 0)
            {
                while (_frameQueue.TryTake(out var frame))
                {
                    frameDataRawBytes.Clear();

                    var hasData = false;

                    // ReSharper disable once LoopCanBeConvertedToQuery
                    foreach (var module in _frameDataRecorderModules)
                    {
                        hasData |= module.SerializeFrameData(frame, frameDataWriter);
                    }

                    if (!hasData) continue;

                    var frameSample = Sample.ProtoBurst.Frame.Pack(frame.FrameNumber, ref frameDataRawBytes, Allocator.Persistent);
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