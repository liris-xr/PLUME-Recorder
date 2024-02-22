using System;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder.Module;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Core.Recorder.Writer;
using PLUME.Core.Scripts;
using Unity.Collections;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PLUME.Core.Recorder
{
    /// <summary>
    /// The main class of the PLUME recorder. It is responsible for managing the recording process (start/stop) and the recorder modules.
    /// It is a singleton and should be accessed through the <see cref="Instance"/> property. The instance is created automatically
    /// after the assemblies are loaded by the application.
    /// </summary>
    public sealed partial class PlumeRecorder : IDisposable
    {
        private RecorderStatus _status = RecorderStatus.Stopped;

        private readonly RecorderContext _context;
        private Record _record;
        private readonly DataDispatcher _dataDispatcher;
        private bool _wantsToQuit;
        
        private readonly long _updateInterval; // in nanoseconds

        private long _lastUpdateTime; // in nanoseconds
        private long _deltaTime; // in nanoseconds
        private bool _shouldUpdate;

        private PlumeRecorder(long updateInterval, DataDispatcher dataDispatcher, RecorderContext ctx)
        {
            _context = ctx;
            _dataDispatcher = dataDispatcher;
            _updateInterval = updateInterval;
        }

        /// <summary>
        /// Starts the recording process. If the recorder is already recording, throw a <see cref="InvalidOperationException"/> exception.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        private void StartRecordingInternal(RecordIdentifier recordIdentifier)
        {
            if (_status is RecorderStatus.Stopping)
                throw new InvalidOperationException(
                    "Recorder is stopping. You cannot start it again until it is stopped.");

            if (_status is RecorderStatus.Recording)
                throw new InvalidOperationException("Recorder is already recording.");

            ApplicationPauseDetector.EnsureExists();

            var recordClock = new Clock();
            _record = new Record(recordClock, recordIdentifier, Allocator.Persistent);

            _lastUpdateTime = 0;
            
            _status = RecorderStatus.Recording;

            _dataDispatcher.Start(_record);

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _context.Modules.Count; i++)
            {
                _context.Modules[i].StartRecording(_record, _context);
            }

            recordClock.Start();

            Logger.Log("Recorder started.");
        }

        /// <summary>
        /// Stops the recording process. If the recorder is not recording, throw a <see cref="InvalidOperationException"/> exception.
        /// This method calls the <see cref="IRecorderModule.StopRecording"/> method on all the recorder modules and stops the <see cref="FrameRecorder"/>.
        /// This method also stops the clock without resetting it.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if called when the recorder is not recording.</exception>
        private async UniTask StopRecordingInternal()
        {
            // TODO: Should wait for end of frame to stop recording (?)
            
            if (_status is RecorderStatus.Stopping)
                throw new InvalidOperationException("Recorder is already stopping.");

            if (_status is not RecorderStatus.Recording)
                throw new InvalidOperationException("Recorder is not recording.");

            Logger.Log("Stopping recorder...");

            _shouldUpdate = false;
            _record.InternalClock.Stop();
            _status = RecorderStatus.Stopping;

            if (_context.TryGetRecorderModule(out FrameRecorderModule frameRecorderModule))
            {
                await frameRecorderModule.CompleteSerializationAsync();
            }

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _context.Modules.Count; i++)
            {
                _context.Modules[i].StopRecording(_record, _context);
            }

            await _dataDispatcher.StopAsync();

            _status = RecorderStatus.Stopped;

            ApplicationPauseDetector.Destroy();
            _record.Dispose();
            _record = null;

            Logger.Log("Recorder stopped.");
        }

        private void ForceStopRecordingInternal()
        {
            if (_status is RecorderStatus.Stopped)
                throw new InvalidOperationException("Recorder is already stopped.");

            Logger.Log("Force stopping recorder...");

            var stopwatch = Stopwatch.StartNew();

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _context.Modules.Count; i++)
            {
                _context.Modules[i].StopRecording(_record, _context);
            }

            _shouldUpdate = false;
            _dataDispatcher.Stop();
            _status = RecorderStatus.Stopped;

            _record.Dispose();
            _record = null;

            stopwatch.Stop();
            Logger.Log("Recorder force stopped after " + stopwatch.ElapsedMilliseconds + "ms.");

            Logger.Log("Recorder force stopped.");
        }

        private void StartRecordingObjectInternal<T>(ObjectSafeRef<T> objectSafeRef, bool markCreated)
            where T : UnityEngine.Object
        {
            EnsureIsRecording();

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _context.Modules.Count; i++)
            {
                var module = _context.Modules[i];
                if (module is not IObjectRecorderModule objectRecorderModule)
                    continue;

                if (objectRecorderModule.SupportedObjectType != objectSafeRef.ObjectType)
                    continue;

                if (objectRecorderModule.IsRecordingObject(objectSafeRef))
                    continue;

                objectRecorderModule.StartRecordingObject(objectSafeRef, markCreated);
            }
        }

        private void StopRecordingObjectInternal<T>(ObjectSafeRef<T> objectSafeRef, bool markDestroyed)
            where T : UnityEngine.Object
        {
            EnsureIsRecording();

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _context.Modules.Count; i++)
            {
                var module = _context.Modules[i];

                if (module is not IObjectRecorderModule objectRecorderModule)
                    continue;

                if (objectRecorderModule.SupportedObjectType != objectSafeRef.ObjectType)
                    continue;

                if (!objectRecorderModule.IsRecordingObject(objectSafeRef))
                    continue;

                objectRecorderModule.StopRecordingObject(objectSafeRef, markDestroyed);
            }
        }

        private void OnApplicationPaused()
        {
            _dataDispatcher?.OnApplicationPaused();
        }

        private bool OnApplicationWantsToQuit()
        {
            if (Application.isEditor)
                return true;

            if (_status is RecorderStatus.Stopped)
                return true;

            if (_status is RecorderStatus.Recording)
            {
                var stopTask = StopRecordingInternal();
                if (!stopTask.Status.IsCompleted())
                {
                    Logger.Log(
                        "Waiting for the recorder modules to stop before quitting the application. The application will stop automatically when finished.");
                }
            }

            if (_wantsToQuit)
                return false;

            UniTask.WaitUntil(() => _status is RecorderStatus.Stopped).ContinueWith(Application.Quit).Forget();
            _wantsToQuit = true;
            return false;
        }

        private void OnApplicationQuitting()
        {
            if (_status is RecorderStatus.Recording or RecorderStatus.Stopping)
                ForceStopRecordingInternal();
        }
        
        private void Awake()
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _context.Modules.Count; i++)
            {
                _context.Modules[i].Awake(_context);
            }
        }
        
        private void EarlyUpdate()
        {
            if (_status is not RecorderStatus.Recording)
            {
                _shouldUpdate = false;
                return;
            }

            var time = _record.Clock.ElapsedNanoseconds;
            var dt = time - _lastUpdateTime;
            
            var nextFrameTime = time + Time.unscaledDeltaTime * 1_000_000_000;
            var nextFrameDt = nextFrameTime - _lastUpdateTime;
            
            // If the next frame is closer to the update interval than the current frame, wait for next frame
            if (Math.Abs(nextFrameDt - _updateInterval) < Math.Abs(dt - _updateInterval))
            {
                _shouldUpdate = false;
                return;
            }
            
            _deltaTime = dt;
            _shouldUpdate = true;
            _lastUpdateTime = time;
            
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _context.Modules.Count; i++)
            {
                _context.Modules[i].EarlyUpdate(_deltaTime, _record, _context);
            }
        }
        
        private void PreUpdate()
        {
            if (!_shouldUpdate || _status is not RecorderStatus.Recording)
                return;
            
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _context.Modules.Count; i++)
            {
                _context.Modules[i].PreUpdate(_deltaTime, _record, _context);
            }
        }
        
        private void Update()
        {
            if (!_shouldUpdate || _status is not RecorderStatus.Recording)
                return;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _context.Modules.Count; i++)
            {
                _context.Modules[i].Update(_deltaTime, _record, _context);
            }
        }

        private void PreLateUpdate()
        {
            if (!_shouldUpdate || _status is not RecorderStatus.Recording)
                return;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _context.Modules.Count; i++)
            {
                _context.Modules[i].PreLateUpdate(_deltaTime, _record, _context);
            }
        }

        private void PostLateUpdate()
        {
            if (!_shouldUpdate || _status is not RecorderStatus.Recording)
                return;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _context.Modules.Count; i++)
            {
                _context.Modules[i].PostLateUpdate(_deltaTime, _record, _context);
            }
        }

        /// <summary>
        /// Ensures that the recorder is recording. If it is not, an <see cref="InvalidOperationException"/> is thrown.
        /// Only called by internal methods.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the recorder is not recording.</exception>
        private void EnsureIsRecording()
        {
            if (_status is not RecorderStatus.Recording)
                throw new InvalidOperationException("Recorder is not recording.");
        }

        public void Dispose()
        {
            foreach (var module in _context.Modules)
                module.Destroy(_context);
        }
    }
}