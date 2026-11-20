using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Google.Protobuf;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder.Module;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Core.Recorder.Writer;
using PLUME.Core.Scripts;
using PLUME.Core.Settings;
using PLUME.Sample;
using PLUME.Sample.Common;
using PLUME.Sample.Unity.Settings;
using ProtoBurst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Pool;
using static PLUME.Core.Utils.SampleUtils;

namespace PLUME.Core.Recorder
{
    /// <summary>
    /// The main class of the PLUME recorder. It is responsible for managing the recording process (start/stop) and the recorder modules.
    /// It is a singleton and should be accessed through the <see cref="Instance"/> property. The instance is created automatically
    /// after the assemblies are loaded by the application.
    /// </summary>
    public sealed partial class PlumeRecorder : IDisposable
    {
        public static readonly RecorderVersion Version = new()
        {
            Name = "PLUME Recorder (Beta)",
            Major = "3",
            Minor = "1",
            Patch = "1",
        };

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
        private void StartRecordingInternal(string name, string extraMetadata = "")
        {
            if (_context.Status is RecorderStatus.Stopping)
                throw new InvalidOperationException(
                    "Recorder is stopping. You cannot start it again until it is stopped.");

            if (_context.Status is RecorderStatus.Recording)
                throw new InvalidOperationException("Recorder is already recording.");

            ErrorReporter.Reset();
            _context.Faults.ResetForNewRecording();
            ApplicationPauseDetector.EnsureExists();

            var createdAt = DateTime.UtcNow;
            var recordMetadata = new RecordMetadata(name, extraMetadata, createdAt);
            var recordClock = new Clock();
            var record = new Record(recordClock, recordMetadata, Allocator.Persistent);

            _context.CurrentRecord = record;
            _context.Status = RecorderStatus.Recording;

            try
            {
                // Record the metadata first
                record.RecordTimelessManagedSample(recordMetadata.ToPayload());
                // Record the application settings (ie. graphics settings, quality settings, etc.)
                RecordApplicationGlobalSettings(record);
                RecordApplicationCurrentSettings(record);

                _dataDispatcher.Start(_context.CurrentRecord);
            }
            catch
            {
                // No module is recording yet: roll back to a clean stopped state and let the caller know.
                _dataDispatcher.Stop();
                ApplicationPauseDetector.Destroy();
                EndRecording();
                throw;
            }

            CallModules(nameof(IRecorderModule.StartRecording), (module, ctx) => module.StartRecording(ctx));

            recordClock.Start();

            Logger.Log("Recorder started.");
        }

        private static void RecordApplicationGlobalSettings(Record record)
        {
            var graphicsSettingsSample = new GraphicsSettings
            {
                DefaultRenderPipelineAsset =
                    GetAssetIdentifierPayload(UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline),
                ColorSpace = QualitySettings.activeColorSpace.ToPayload()
            };

            record.RecordTimelessManagedSample(graphicsSettingsSample);
        }

        private static void RecordApplicationCurrentSettings(Record record)
        {
            var qualityLevel = QualitySettings.GetQualityLevel();

            var qualitySettingsUpdateSample = new QualitySettingsUpdate
            {
                Name = QualitySettings.names[qualityLevel],
                RenderPipelineAsset = GetAssetIdentifierPayload(QualitySettings.renderPipeline)
            };

            var audioSettingsUpdateSample = new AudioSettingsUpdate
            {
                SpeakerMode = AudioSettings.speakerMode.ToPayload(),
                SpatializerPluginName = AudioSettings.GetSpatializerPluginName()
            };

            record.RecordTimestampedManagedSample(qualitySettingsUpdateSample);
            record.RecordTimestampedManagedSample(audioSettingsUpdateSample);
        }

        /// <summary>
        /// Stops the recording process. If the recorder is not recording, throw a <see cref="InvalidOperationException"/> exception.
        /// This method calls the <see cref="IRecorderModule.StopRecording"/> method on all the recorder modules and stops the <see cref="FrameRecorder"/>.
        /// This method also stops the clock without resetting it.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if called when the recorder is not recording.</exception>
        private async Task StopRecordingInternal()
        {
            if (_context.Status is RecorderStatus.Stopping)
                throw new InvalidOperationException("Recorder is already stopping.");

            if (_context.Status is not RecorderStatus.Recording)
                throw new InvalidOperationException("Recorder is not recording.");

            Logger.Log("Stopping recorder...");

            _context.CurrentRecord.InternalClock.Stop();
            _context.Status = RecorderStatus.Stopping;

            try
            {
                if (_context.TryGetRecorderModule(out FrameRecorderModule frameRecorderModule))
                {
                    await frameRecorderModule.CompleteSerializationAsync();
                }

                CallModules(nameof(IRecorderModule.StopRecording), (module, ctx) => module.StopRecording(ctx));

                await _dataDispatcher.StopAsync();
            }
            finally
            {
                ApplicationPauseDetector.Destroy();
                EndRecording();
            }

            Logger.Log("Recorder stopped.");
        }

        private void ForceStopRecordingInternal()
        {
            if (_context.Status is RecorderStatus.Stopped)
                throw new InvalidOperationException("Recorder is already stopped.");

            Logger.Log("Force stopping recorder...");
            _context.Status = RecorderStatus.Stopping;

            var stopwatch = Stopwatch.StartNew();

            try
            {
                CallModules(nameof(IRecorderModule.StopRecording), (module, ctx) => module.StopRecording(ctx));
                _dataDispatcher.Stop();
            }
            finally
            {
                EndRecording();
            }

            stopwatch.Stop();
            Logger.Log("Recorder force stopped after " + stopwatch.ElapsedMilliseconds + "ms.");
        }

        /// <summary>
        /// Releases the current record and marks the recorder as stopped. Runs whatever happened before, so that the
        /// recorder never stays stuck in <see cref="RecorderStatus.Stopping"/> nor leaks the record's native memory.
        /// </summary>
        private void EndRecording()
        {
            _context.Status = RecorderStatus.Stopped;
            _context.CurrentRecord?.Dispose();
            _context.CurrentRecord = null;
        }

        /// <summary>
        /// Calls a lifecycle method on every module. This is the exception boundary between the recorder and its
        /// modules: a module that throws is disabled and the remaining modules are still called.
        /// </summary>
        /// <param name="phase">Name of the lifecycle method, used to report failures.</param>
        /// <param name="call">Calls the lifecycle method on the given module.</param>
        /// <param name="includeUninitialized">Whether to also call modules that failed to initialize, e.g. to let
        /// them release what they allocated before failing.</param>
        private void CallModules(string phase, Action<IRecorderModule, RecorderContext> call,
            bool includeUninitialized = false)
        {
            // A module failing before any recording started is not initialized, so it is never started.
            var disablesPermanently = phase is nameof(IRecorderModule.Create) or nameof(IRecorderModule.Awake);

            foreach (var module in _context.Modules)
            {
                if (!includeUninitialized && _context.Faults.IsDisabledPermanently(module))
                    continue;

                try
                {
                    call(module, _context);
                }
                catch (Exception e)
                {
                    _context.Faults.Disable(module, phase, e, disablesPermanently);
                }
            }
        }
        
        private void RecordTimestampedManagedSampleInternal(IMessage sample)
        {
            EnsureIsRecording();
            _context.CurrentRecord.RecordTimestampedManagedSample(sample);
        }

        private void RecordTimestampedSampleInternal<T>(T msg) where T : unmanaged, IProtoBurstMessage
        {
            EnsureIsRecording();
            _context.CurrentRecord.RecordTimestampedSample(msg);
        }
        
        private void RecordTimelessManagedSampleInternal(IMessage msg)
        {
            EnsureIsRecording();
            _context.CurrentRecord.RecordTimelessManagedSample(msg);
        }

        private void RecordTimelessSampleInternal<T>(T msg) where T : unmanaged, IProtoBurstMessage
        {
            EnsureIsRecording();
            _context.CurrentRecord.RecordTimelessSample(msg);
        }

        private void RecordMarkerInternal(string label)
        {
            EnsureIsRecording();
            var marker = new Marker { Label = label };
            _context.CurrentRecord.RecordTimestampedManagedSample(marker);
        }

        private void OnApplicationPaused()
        {
            _dataDispatcher?.OnApplicationPaused();
        }

        private bool OnApplicationWantsToQuit()
        {
            if (Application.isEditor)
                return true;

            if (_context.Status is RecorderStatus.Stopped)
                return true;

            if (_context.Status is RecorderStatus.Recording)
            {
                var stopTask = StopRecordingInternal();
                // Unlike UniTask.Forget(), an unawaited Task swallows its exceptions.
                stopTask.ContinueWith(t => Logger.LogException(t.Exception),
                    TaskContinuationOptions.OnlyOnFaulted);
                if (!stopTask.IsCompleted)
                {
                    Logger.Log(
                        "Waiting for the recorder modules to stop before quitting the application. The application will stop automatically when finished.");
                }
            }

            if (_wantsToQuit)
                return false;

            QuitWhenStopped();
            _wantsToQuit = true;
            return false;
        }

        private async void QuitWhenStopped()
        {
            // Task.Yield resumes on the next frame through UnitySynchronizationContext.
            while (_context.Status is not RecorderStatus.Stopped)
                await Task.Yield();
            Application.Quit();
        }

        private void OnApplicationQuitting()
        {
            if (_context.Status is RecorderStatus.Recording or RecorderStatus.Stopping)
                ForceStopRecordingInternal();
        }

        private void Awake()
        {
            CallModules(nameof(IRecorderModule.Awake), (module, ctx) => module.Awake(ctx));

            var recorderSettings = _context.SettingsProvider.GetOrCreate<RecorderSettings>();

            if (!recorderSettings.StartOnPlay)
                return;

            // Called by Unity at scene load: report the failure instead of throwing into the engine.
            try
            {
                StartRecordingInternal(recorderSettings.DefaultRecordPrefix, recorderSettings.DefaultRecordExtraMetadata);
            }
            catch (Exception e)
            {
                ErrorReporter.Report($"{nameof(PlumeRecorder)}.{nameof(RecorderSettings.StartOnPlay)}",
                    "Failed to start recording on play. Nothing is recorded.", e);
            }
        }

        /// <summary>
        /// Ensures that the recorder is recording. If it is not, an <see cref="InvalidOperationException"/> is thrown.
        /// Only called by internal methods.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the recorder is not recording.</exception>
        private void EnsureIsRecording()
        {
            if (_context.Status is not RecorderStatus.Recording)
                throw new InvalidOperationException("Recorder is not recording.");
        }

        public void Dispose()
        {
            CallModules(nameof(IRecorderModule.Destroy), (module, ctx) => module.Destroy(ctx),
                includeUninitialized: true);
        }
    }
}