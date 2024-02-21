using System;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder.Module;
using PLUME.Core.Recorder.Writer;
using PLUME.Core.Scripts;
using Unity.Collections;
using UnityEngine;

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

        private PlumeRecorder(DataDispatcher dataDispatcher, RecorderContext ctx)
        {
            _context = ctx;
            _dataDispatcher = dataDispatcher;
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
            if (_status is RecorderStatus.Stopping)
                throw new InvalidOperationException("Recorder is already stopping.");

            if (_status is not RecorderStatus.Recording)
                throw new InvalidOperationException("Recorder is not recording.");

            Logger.Log("Stopping recorder...");

            _record.InternalClock.Stop();
            _status = RecorderStatus.Stopping;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _context.Modules.Count; i++)
            {
                await _context.Modules[i].StopRecording(_record, _context);
            }

            await _dataDispatcher.Stop();

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
                _context.Modules[i].ForceStopRecording(_record, _context);
            }

            _dataDispatcher.ForceStop();
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

        private void PreUpdate()
        {
            if (_status is not RecorderStatus.Recording)
                return;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _context.Modules.Count; i++)
            {
                _context.Modules[i].PreUpdate(_record, _context);
            }
        }

        private void EarlyUpdate()
        {
            if (_status is not RecorderStatus.Recording)
                return;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _context.Modules.Count; i++)
            {
                _context.Modules[i].EarlyUpdate(_record, _context);
            }
        }

        private void Update()
        {
            if (_status is not RecorderStatus.Recording)
                return;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _context.Modules.Count; i++)
            {
                _context.Modules[i].Update(_record, _context);
            }
        }

        private void PreLateUpdate()
        {
            if (_status is not RecorderStatus.Recording)
                return;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _context.Modules.Count; i++)
            {
                _context.Modules[i].PreLateUpdate(_record, _context);
            }
        }

        private void PostLateUpdate()
        {
            if (_status is not RecorderStatus.Recording)
                return;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _context.Modules.Count; i++)
            {
                _context.Modules[i].PostLateUpdate(_record, _context);
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