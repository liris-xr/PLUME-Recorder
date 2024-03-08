using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using PLUME.Core.Settings;
using PLUME.Core.Utils;
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
        private IFrameDataRecorderModule[] _frameDataRecorderModules;

        private Thread _serializationThread;
        private bool _shouldSerialize;

        private BlockingCollection<FrameInfo> _frameQueue;

        private ulong _updateInterval; // in nanoseconds

        private ulong _lastUpdateTime; // in nanoseconds
        private ulong _lastFixedUpdateTime; // in nanoseconds
        private ulong _deltaTime; // in nanoseconds
        private bool _shouldRunUpdate;

        void IRecorderModule.Create(RecorderContext ctx)
        {
            _frameQueue = new BlockingCollection<FrameInfo>(new ConcurrentQueue<FrameInfo>());
            _frameDataRecorderModules = ctx.Modules.OfType<IFrameDataRecorderModule>().ToArray();

            var settings = ctx.SettingsProvider.GetOrCreate<FrameRecorderModuleSettings>();
            _updateInterval = (ulong)(1_000_000_000 / settings.UpdateRate);

            PlayerLoopUtils.InjectEarlyUpdate<RecorderEarlyUpdate>(() => EarlyUpdate(ctx));
            PlayerLoopUtils.InjectPreUpdate<RecorderPreUpdate>(() => PreUpdate(ctx));
            PlayerLoopUtils.InjectUpdate<RecorderUpdate>(() => Update(ctx));
            PlayerLoopUtils.InjectPreLateUpdate<RecorderPreLateUpdate>(() => PreLateUpdate(ctx));
            PlayerLoopUtils.InjectPostLateUpdate<RecorderPostLateUpdate>(() => PostLateUpdate(ctx));
        }

        void IRecorderModule.Destroy(RecorderContext ctx)
        {
            _frameQueue.Dispose();
        }

        void IRecorderModule.Awake(RecorderContext ctx)
        {
        }

        void IRecorderModule.StartRecording(RecorderContext ctx)
        {
            _lastUpdateTime = 0;
            _shouldRunUpdate = true;
            _shouldSerialize = true;

            _serializationThread = new Thread(() => SerializeFrameLoop(ctx.CurrentRecord))
            {
                Name = "FrameRecorderModule.SerializeThread",
                IsBackground = false
            };

            _serializationThread.Start();
        }

        void IRecorderModule.StopRecording(RecorderContext ctx)
        {
            _shouldRunUpdate = false;
            _shouldSerialize = false;
            _serializationThread.Join();
            _serializationThread = null;
        }

        internal async UniTask CompleteSerializationAsync()
        {
            _shouldSerialize = false;

            var remainingFramesCount = _frameQueue.Count;
            if (remainingFramesCount > 0)
                Logger.Log(nameof(FrameRecorderModule), $"{remainingFramesCount} frames left in queue.");

            await UniTask.WaitUntil(() => !_serializationThread.IsAlive);
        }

        private void EarlyUpdate(RecorderContext ctx)
        {
            UpdateShouldRunUpdateFlag(ctx);
            RunFixedUpdate(ctx);

            if (!_shouldRunUpdate || !ctx.IsRecording)
                return;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _frameDataRecorderModules.Length; i++)
            {
                _frameDataRecorderModules[i].EarlyUpdate(_deltaTime, ctx);
            }
        }

        private void RunFixedUpdate(RecorderContext ctx)
        {
            if (_updateInterval == 0 || !ctx.IsRecording)
                return;
            
            var time = ctx.CurrentRecord.Time;
            var fixedUpdateDt = time - _lastFixedUpdateTime;
            
            if (fixedUpdateDt < _updateInterval)
                return;
            
            var nFixedUpdates = (int) (fixedUpdateDt / _updateInterval);
            var fixedTime = _lastFixedUpdateTime;

            ctx.CurrentRecord.FixedTime = fixedTime;

            for (var i = 0; i < nFixedUpdates; i++)
            {
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var j = 0; j < _frameDataRecorderModules.Length; j++)
                {
                    _frameDataRecorderModules[j].FixedUpdate(_updateInterval, ctx);
                }

                fixedTime += _updateInterval;
                ctx.CurrentRecord.FixedTime = fixedTime;
            }

            _lastFixedUpdateTime = fixedTime;
        }

        private void UpdateShouldRunUpdateFlag(RecorderContext ctx)
        {
            if (_updateInterval == 0 || !ctx.IsRecording)
            {
                _shouldRunUpdate = false;
                return;
            }

            var time = ctx.CurrentRecord.Time;
            var updateDt = time - _lastUpdateTime;
            var nextUpdateDt = (ulong) (time + Time.unscaledTimeAsDouble * 1_000_000_000 - _lastUpdateTime);

            // If the next frame is closer to the update interval than the current frame, wait for next frame
            if (Math.Abs((long) nextUpdateDt - (long) _updateInterval) < Math.Abs((long) updateDt - (long) _updateInterval))
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

        private void PreUpdate(RecorderContext ctx)
        {
            if (!_shouldRunUpdate || !ctx.IsRecording)
                return;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _frameDataRecorderModules.Length; i++)
            {
                _frameDataRecorderModules[i].PreUpdate(_deltaTime, ctx);
            }
        }

        private void Update(RecorderContext ctx)
        {
            if (!_shouldRunUpdate || !ctx.IsRecording)
                return;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _frameDataRecorderModules.Length; i++)
            {
                _frameDataRecorderModules[i].Update(_deltaTime, ctx);
            }
        }

        private void PreLateUpdate(RecorderContext ctx)
        {
            if (!_shouldRunUpdate || !ctx.IsRecording)
                return;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _frameDataRecorderModules.Length; i++)
            {
                _frameDataRecorderModules[i].PreLateUpdate(_deltaTime, ctx);
            }
        }

        private void PostLateUpdate(RecorderContext ctx)
        {
            if (!_shouldRunUpdate || !ctx.IsRecording)
                return;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _frameDataRecorderModules.Length; i++)
            {
                _frameDataRecorderModules[i].PostLateUpdate(_deltaTime, ctx);
            }

            var timestamp = ctx.CurrentRecord.Time;
            var frame = Time.frameCount;
            PushFrame(timestamp, frame, ctx);
        }

        private void PushFrame(ulong timestamp, int frameNumber, RecorderContext ctx)
        {
            var frame = new FrameInfo(timestamp, frameNumber);

            foreach (var module in _frameDataRecorderModules)
            {
                module.BeforeEnqueueFrameData(frame, ctx);
            }
            
            foreach (var module in _frameDataRecorderModules)
            {
                module.EnqueueFrameData(frame, ctx);
            }
            
            _frameQueue.Add(frame);
            
            foreach (var module in _frameDataRecorderModules)
            {
                module.AfterEnqueueFrameData(frame, ctx);
            }
        }

        internal void SerializeFrameLoop(Record record)
        {
            Profiler.BeginThreadProfiling("PLUME", "FrameRecorderModule.SerializeThread");

            // This buffer is reused for each frame.
            var frameDataRawBytes = new NativeList<byte>(Allocator.Persistent);
            var frameDataWriter = new FrameDataWriter(frameDataRawBytes);

            var frameSampleTypeUrl = SampleTypeUrl.Alloc(Sample.ProtoBurst.Unity.Frame.TypeUrl, Allocator.Persistent);

            while (_shouldSerialize || _frameQueue.Count > 0)
            {
                while (_frameQueue.TryTake(out var frame))
                {
                    frameDataRawBytes.Clear();

                    // ReSharper disable once LoopCanBeConvertedToQuery
                    foreach (var module in _frameDataRecorderModules)
                    {
                        module.SerializeFrameData(frame, frameDataWriter);
                    }

                    var frameSample = Sample.ProtoBurst.Unity.Frame.Pack(frame.FrameNumber, ref frameDataRawBytes, Allocator.Persistent);
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