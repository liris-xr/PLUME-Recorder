using System;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder.Module;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Core.Recorder.Writer;
using PLUME.Core.Scripts;
using PLUME.Sample.Common;
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
            _context.IsRecording = true;
            _context.CurrentRecord = new Record(recordClock, recordIdentifier, Allocator.Persistent);
            
            _status = RecorderStatus.Recording;

            _dataDispatcher.Start(_context.CurrentRecord);

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _context.Modules.Count; i++)
            {
                _context.Modules[i].StartRecording(_context);
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

            _context.CurrentRecord.InternalClock.Stop();
            _status = RecorderStatus.Stopping;

            if (_context.TryGetRecorderModule(out FrameRecorderModule frameRecorderModule))
            {
                await frameRecorderModule.CompleteSerializationAsync();
            }

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _context.Modules.Count; i++)
            {
                _context.Modules[i].StopRecording(_context);
            }

            await _dataDispatcher.StopAsync();

            _status = RecorderStatus.Stopped;

            ApplicationPauseDetector.Destroy();
            _context.IsRecording = true;

            if (_context.CurrentRecord != null)
            {
                _context.CurrentRecord.Dispose();
                _context.CurrentRecord = null;
            }

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
                _context.Modules[i].StopRecording(_context);
            }

            _dataDispatcher.Stop();
            _status = RecorderStatus.Stopped;

            _context.IsRecording = false;
            if (_context.CurrentRecord != null)
            {
                _context.CurrentRecord.Dispose();
                _context.CurrentRecord = null;
            }

            stopwatch.Stop();
            Logger.Log("Recorder force stopped after " + stopwatch.ElapsedMilliseconds + "ms.");

            Logger.Log("Recorder force stopped.");
        }

        private void StartRecordingObjectInternal(IObjectSafeRef objectSafeRef, bool markCreated)
        {
            EnsureIsRecording();

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _context.Modules.Count; i++)
            {
                var module = _context.Modules[i];
                if (module is not IObjectRecorderModule objectRecorderModule)
                    continue;

                if(!objectRecorderModule.IsObjectSupported(objectSafeRef))
                    continue;

                if (objectRecorderModule.IsRecordingObject(objectSafeRef))
                    continue;

                objectRecorderModule.StartRecordingObject(objectSafeRef, markCreated, _context);
            }
        }

        private void StopRecordingObjectInternal(IObjectSafeRef objectSafeRef, bool markDestroyed)
        {
            EnsureIsRecording();

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _context.Modules.Count; i++)
            {
                var module = _context.Modules[i];

                if (module is not IObjectRecorderModule objectRecorderModule)
                    continue;

                if(!objectRecorderModule.IsObjectSupported(objectSafeRef))
                    continue;

                if (!objectRecorderModule.IsRecordingObject(objectSafeRef))
                    continue;

                objectRecorderModule.StopRecordingObject(objectSafeRef, markDestroyed, _context);
            }
        }
        
        private void RecordMarkerInternal(string label)
        {
            EnsureIsRecording();
            var marker = new Marker { Label = label };
            _context.CurrentRecord.RecordTimestampedSample(marker);
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