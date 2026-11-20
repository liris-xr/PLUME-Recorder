using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            // Each record has its own clock starting at 0: times left over from a previous recording would make
            // the elapsed time underflow and schedule billions of fixed updates.
            _lastUpdateTime = 0;
            _lastFixedUpdateTime = 0;
            _shouldRunUpdate = true;
            _shouldSerialize = true;

            _serializationThread = new Thread(() => SerializeFrameLoop(ctx, ctx.CurrentRecord))
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
            _serializationThread?.Join();
            _serializationThread = null;

            // Frames left behind by a failed serialization must not leak into the next recording.
            while (_frameQueue.TryTake(out _))
            {
            }

            foreach (var module in _frameDataRecorderModules)
            {
                try
                {
                    module.DisposePendingFrameData();
                }
                catch (Exception e)
                {
                    ctx.Faults.Disable(module, nameof(IFrameDataRecorderModule.DisposePendingFrameData), e);
                }
            }
        }

        internal async Task CompleteSerializationAsync()
        {
            _shouldSerialize = false;

            if (_serializationThread == null)
                return;

            var remainingFramesCount = _frameQueue.Count;
            if (remainingFramesCount > 0)
                Logger.Log(nameof(FrameRecorderModule), $"{remainingFramesCount} frames left in queue.");

            await Task.Run(_serializationThread.Join);
        }

        private enum Phase
        {
            FixedUpdate,
            EarlyUpdate,
            PreUpdate,
            Update,
            PreLateUpdate,
            PostLateUpdate,
            BeforeEnqueueFrameData,
            EnqueueFrameData,
            AfterEnqueueFrameData
        }

        /// <summary>
        /// Calls the given phase on every enabled frame data recorder module. This is the exception boundary
        /// between the frame loop and the modules: a module that throws is disabled, the others still run.
        /// </summary>
        private void RunPhase(Phase phase, ulong deltaTime, FrameInfo frame, RecorderContext ctx)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _frameDataRecorderModules.Length; i++)
            {
                var module = _frameDataRecorderModules[i];

                if (ctx.Faults.IsDisabled(module))
                    continue;

                try
                {
                    switch (phase)
                    {
                        case Phase.FixedUpdate:
                            module.FixedUpdate(deltaTime, ctx);
                            break;
                        case Phase.EarlyUpdate:
                            module.EarlyUpdate(deltaTime, ctx);
                            break;
                        case Phase.PreUpdate:
                            module.PreUpdate(deltaTime, ctx);
                            break;
                        case Phase.Update:
                            module.Update(deltaTime, ctx);
                            break;
                        case Phase.PreLateUpdate:
                            module.PreLateUpdate(deltaTime, ctx);
                            break;
                        case Phase.PostLateUpdate:
                            module.PostLateUpdate(deltaTime, ctx);
                            break;
                        case Phase.BeforeEnqueueFrameData:
                            module.BeforeEnqueueFrameData(frame, ctx);
                            break;
                        case Phase.EnqueueFrameData:
                            module.EnqueueFrameData(frame, ctx);
                            break;
                        case Phase.AfterEnqueueFrameData:
                            module.AfterEnqueueFrameData(frame, ctx);
                            break;
                    }
                }
                catch (Exception e)
                {
                    ctx.Faults.Disable(module, phase.ToString(), e);
                }
            }
        }

        private void EarlyUpdate(RecorderContext ctx)
        {
            UpdateShouldRunUpdateFlag(ctx);
            RunFixedUpdate(ctx);

            if (!_shouldRunUpdate || !ctx.IsRecording)
                return;

            RunPhase(Phase.EarlyUpdate, _deltaTime, default, ctx);
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
                RunPhase(Phase.FixedUpdate, _updateInterval, default, ctx);

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

            RunPhase(Phase.PreUpdate, _deltaTime, default, ctx);
        }

        private void Update(RecorderContext ctx)
        {
            if (!_shouldRunUpdate || !ctx.IsRecording)
                return;

            RunPhase(Phase.Update, _deltaTime, default, ctx);
        }

        private void PreLateUpdate(RecorderContext ctx)
        {
            if (!_shouldRunUpdate || !ctx.IsRecording)
                return;

            RunPhase(Phase.PreLateUpdate, _deltaTime, default, ctx);
        }

        private void PostLateUpdate(RecorderContext ctx)
        {
            if (!_shouldRunUpdate || !ctx.IsRecording)
                return;

            RunPhase(Phase.PostLateUpdate, _deltaTime, default, ctx);

            var timestamp = ctx.CurrentRecord.Time;
            var frame = Time.frameCount;
            PushFrame(timestamp, frame, ctx);
        }

        private void PushFrame(ulong timestamp, int frameNumber, RecorderContext ctx)
        {
            var frame = new FrameInfo(timestamp, frameNumber);

            RunPhase(Phase.BeforeEnqueueFrameData, 0, frame, ctx);
            // Objects found destroyed before collection are recorded destroyed in this very frame.
            ctx.StopRecordingDestroyedObjectsIfRequested();
            RunPhase(Phase.EnqueueFrameData, 0, frame, ctx);
            _frameQueue.Add(frame);
            RunPhase(Phase.AfterEnqueueFrameData, 0, frame, ctx);
        }

        internal void SerializeFrameLoop(RecorderContext ctx, Record record)
        {
            Profiler.BeginThreadProfiling("PLUME", "FrameRecorderModule.SerializeThread");

            // This buffer is reused for each frame.
            var frameDataRawBytes = new NativeList<byte>(Allocator.Persistent);
            var frameDataWriter = new FrameDataWriter(frameDataRawBytes);

            var frameSampleTypeUrl = SampleTypeUrl.Alloc(Sample.ProtoBurst.Unity.Frame.TypeUrl, Allocator.Persistent);

            try
            {
                while (_shouldSerialize || _frameQueue.Count > 0)
                {
                    while (_frameQueue.TryTake(out var frame))
                    {
                        // A frame that fails to serialize is dropped: the thread must survive to serialize the next ones.
                        try
                        {
                            SerializeFrame(frame, ctx, record, ref frameDataRawBytes, frameDataWriter, frameSampleTypeUrl);
                        }
                        catch (Exception e)
                        {
                            ErrorReporter.Report($"{nameof(FrameRecorderModule)}.{nameof(SerializeFrame)}",
                                "Failed to serialize a frame. The frame is dropped from the record.", e);
                        }
                    }
                }
            }
            finally
            {
                frameSampleTypeUrl.Dispose();
                frameDataRawBytes.Dispose();
                Profiler.EndThreadProfiling();
            }
        }

        private void SerializeFrame(FrameInfo frame, RecorderContext ctx, Record record,
            ref NativeList<byte> frameDataRawBytes, FrameDataWriter frameDataWriter, SampleTypeUrl frameSampleTypeUrl)
        {
            frameDataRawBytes.Clear();

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var module in _frameDataRecorderModules)
            {
                if (ctx.Faults.IsDisabled(module))
                    continue;

                try
                {
                    module.SerializeFrameData(frame, frameDataWriter);
                }
                catch (Exception e)
                {
                    ctx.Faults.Disable(module, nameof(IFrameDataRecorderModule.SerializeFrameData), e);
                }
            }

            using var frameSample = Sample.ProtoBurst.Unity.Frame.Pack(frame.FrameNumber, ref frameDataRawBytes, Allocator.Persistent);
            using var frameSampleBytes = frameSample.ToBytes(Allocator.Persistent);
            record.RecordTimestampedSample(frameSampleBytes, frameSampleTypeUrl, frame.Timestamp);
        }
    }
}