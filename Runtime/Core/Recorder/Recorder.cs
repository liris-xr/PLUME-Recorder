using System;
using System.Collections.Generic;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder.Module;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Core.Recorder.Writer;
using PLUME.Core.Scripts;
using PLUME.Core.Settings;
using PLUME.Sample;
using PLUME.Sample.Common;
using PLUME.Sample.Unity.Settings;
using Unity.Collections;
using UnityEngine;
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
            Major = "1",
            Minor = "0",
            Patch = "1",
        };

        private readonly RecorderContext _context;
        private readonly DataDispatcher _dataDispatcher;
        private bool _wantsToQuit;
        
        private static readonly List<Component> TempComponents = new();

        private PlumeRecorder(DataDispatcher dataDispatcher, RecorderContext ctx)
        {
            _context = ctx;
            _dataDispatcher = dataDispatcher;
        }

        /// <summary>
        /// Starts the recording process. If the recorder is already recording, throw a <see cref="InvalidOperationException"/> exception.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        private void StartRecordingInternal(string name, bool recordAll = true, string extraMetadata = "")
        {
            if (_context.Status is RecorderStatus.Stopping)
                throw new InvalidOperationException(
                    "Recorder is stopping. You cannot start it again until it is stopped.");

            if (_context.Status is RecorderStatus.Recording)
                throw new InvalidOperationException("Recorder is already recording.");

            ApplicationPauseDetector.EnsureExists();

            var createdAt = DateTime.UtcNow;
            var recordMetadata = new RecordMetadata(name, extraMetadata, createdAt);
            var recordClock = new Clock();
            var record = new Record(recordClock, recordMetadata, Allocator.Persistent);

            _context.CurrentRecord = record;
            _context.Status = RecorderStatus.Recording;

            // Record the metadata first
            record.RecordTimelessManagedSample(recordMetadata.ToPayload());
            // Record the application settings (ie. graphics settings, quality settings, etc.)
            RecordApplicationGlobalSettings(record);
            RecordApplicationCurrentSettings(record);

            _dataDispatcher.Start(_context.CurrentRecord);

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _context.Modules.Count; i++)
            {
                _context.Modules[i].StartRecording(_context);
            }

            recordClock.Start();

            if (recordAll)
            {
                foreach (var go in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include,
                             FindObjectsSortMode.None))
                {
                    StartRecordingGameObject(go);
                }
            }

            Logger.Log("Recorder started.");
        }

        private static void RecordApplicationGlobalSettings(Record record)
        {
            var graphicsSettingsSample = new GraphicsSettings
            {
                DefaultRenderPipelineAssetId =
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
                RenderPipelineAssetId = GetAssetIdentifierPayload(QualitySettings.renderPipeline)
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
        private async UniTask StopRecordingInternal()
        {
            if (_context.Status is RecorderStatus.Stopping)
                throw new InvalidOperationException("Recorder is already stopping.");

            if (_context.Status is not RecorderStatus.Recording)
                throw new InvalidOperationException("Recorder is not recording.");

            Logger.Log("Stopping recorder...");

            _context.CurrentRecord.InternalClock.Stop();
            _context.Status = RecorderStatus.Stopping;

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

            ApplicationPauseDetector.Destroy();

            if (_context.CurrentRecord != null)
            {
                _context.CurrentRecord.Dispose();
                _context.CurrentRecord = null;
            }

            _context.Status = RecorderStatus.Stopped;
            Logger.Log("Recorder stopped.");
        }

        private void ForceStopRecordingInternal()
        {
            if (_context.Status is RecorderStatus.Stopped)
                throw new InvalidOperationException("Recorder is already stopped.");

            Logger.Log("Force stopping recorder...");
            _context.Status = RecorderStatus.Stopping;

            var stopwatch = Stopwatch.StartNew();

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _context.Modules.Count; i++)
            {
                _context.Modules[i].StopRecording(_context);
            }

            _dataDispatcher.Stop();

            if (_context.CurrentRecord != null)
            {
                _context.CurrentRecord.Dispose();
                _context.CurrentRecord = null;
            }

            stopwatch.Stop();
            _context.Status = RecorderStatus.Stopped;
            Logger.Log("Recorder force stopped after " + stopwatch.ElapsedMilliseconds + "ms.");
        }
        
        private void StartRecordingGameObjectInternal(GameObject go, bool markCreated = true)
        {
            EnsureIsRecording();
            
            var safeRefProvider = _context.ObjectSafeRefProvider;
            
            go.GetComponentsInChildren(true, TempComponents);
            
            foreach (var component in TempComponents)
            {
                var componentSafeRef = safeRefProvider.GetOrCreateComponentSafeRef(component);

                // Start recording nested GameObjects. This also applies to the given GameObject itself.
                if (component is Transform)
                {
                    StartRecordingObjectInternal(componentSafeRef.GameObjectSafeRef, markCreated);
                }
                
                StartRecordingObjectInternal(componentSafeRef, markCreated);
            }
        }
        
        private void StopRecordingGameObjectInternal(GameObject go, bool markDestroyed = true)
        {
            EnsureIsRecording();
            
            var safeRefProvider = _context.ObjectSafeRefProvider;
            
            go.GetComponentsInChildren(TempComponents);
            
            foreach (var component in TempComponents)
            {
                var componentSafeRef = safeRefProvider.GetOrCreateComponentSafeRef(component);
                StopRecordingObjectInternal(componentSafeRef, markDestroyed);
                
                // Stop recording nested GameObjects. This also applies to the given GameObject itself.
                if (component is Transform)
                {
                    StopRecordingObjectInternal(componentSafeRef.GameObjectSafeRef, markDestroyed);
                }
            }
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

                if (!objectRecorderModule.IsObjectSupported(objectSafeRef))
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

                if (!objectRecorderModule.IsObjectSupported(objectSafeRef))
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
                if (!stopTask.Status.IsCompleted())
                {
                    Logger.Log(
                        "Waiting for the recorder modules to stop before quitting the application. The application will stop automatically when finished.");
                }
            }

            if (_wantsToQuit)
                return false;

            UniTask.WaitUntil(() => _context.Status is RecorderStatus.Stopped).ContinueWith(Application.Quit).Forget();
            _wantsToQuit = true;
            return false;
        }

        private void OnApplicationQuitting()
        {
            if (_context.Status is RecorderStatus.Recording or RecorderStatus.Stopping)
                ForceStopRecordingInternal();
        }

        private void Awake()
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _context.Modules.Count; i++)
            {
                _context.Modules[i].Awake(_context);
            }

            var recorderSettings = _context.SettingsProvider.GetOrCreate<RecorderSettings>();

            if (recorderSettings.StartOnPlay)
            {
                StartRecordingInternal(recorderSettings.DefaultRecordPrefix, true,
                    recorderSettings.DefaultRecordExtraMetadata);
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
            foreach (var module in _context.Modules)
                module.Destroy(_context);
        }
    }
}