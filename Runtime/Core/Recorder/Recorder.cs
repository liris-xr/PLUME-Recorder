using System;
using System.Collections.Generic;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
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
            Minor = "0",
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
        private void StartRecordingInternal(string name, string extraMetadata = "", IDataWriterInfo[] outputInfos = null)
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

            IDataWriter<IDataWriterInfo>[] outputs;

            if (outputInfos == null)
            {
                IDataWriter<IDataWriterInfo> fileDataWriter = (IDataWriter<IDataWriterInfo>) new FileDataWriter(record);
                outputs = new IDataWriter<IDataWriterInfo>[] { fileDataWriter };
            }
            else
            {
                List<IDataWriter<IDataWriterInfo>> outputsList = new List<IDataWriter<IDataWriterInfo>>();
                foreach(IDataWriterInfo info in outputInfos)
                {
                    outputsList.Add(info switch
                        {
                            FileDataWriterInfo => (IDataWriter<IDataWriterInfo>)new FileDataWriter(record, (FileDataWriterInfo)info),
                            NetworkDataWriterInfo => (IDataWriter<IDataWriterInfo>)new NetworkDataWriter(record, (NetworkDataWriterInfo)info),
                            _ => throw new InvalidOperationException()
                        });
                }
                outputs = outputsList.ToArray();
            }

            _dataDispatcher.Start(_context.CurrentRecord, outputs);

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _context.Modules.Count; i++)
            {
                _context.Modules[i].StartRecording(_context);
            }

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
                // TODO: allow configuring data writers in Settings
                StartRecordingInternal(recorderSettings.DefaultRecordPrefix, recorderSettings.DefaultRecordExtraMetadata);
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
