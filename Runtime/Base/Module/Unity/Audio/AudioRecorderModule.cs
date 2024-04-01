using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using NAudio.Wave;
using PLUME.Base.Settings;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module;
using UnityEngine;
using UnityEngine.Scripting;
using Logger = PLUME.Core.Logger;
using Object = UnityEngine.Object;

namespace PLUME.Base.Module.Unity.Audio
{
    [Preserve]
    public class AudioRecorderModule : RecorderModule
    {
        private WaveFileWriter _audioFileWriter;
        private AudioListener _audioListener;

        private int _channelCount;
        private int _outputSampleRate;
        private const double TriggerAudioSyncThresholdSeconds = 0.01; // in seconds

        private Thread _audioRecordingThread;

        private AudioRecorderModuleSettings _settings;

        private readonly BlockingCollection<AudioSamples> _audioSamplesQueue = new(new ConcurrentQueue<AudioSamples>());

        protected override void OnCreate(RecorderContext ctx)
        {
            base.OnCreate(ctx);
            _settings = ctx.SettingsProvider.GetOrCreate<AudioRecorderModuleSettings>();
        }

        protected override void OnStartRecording(RecorderContext ctx)
        {
            base.OnStartRecording(ctx);

            var audioListeners =
                Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            if (!_settings.Enabled)
            {
                Logger.Log("Audio recording is disabled in the settings. No audio recording will be performed.");
                return;
            }

            if (audioListeners.Length == 0)
            {
                Logger.LogWarning("No AudioListener found in the scene. No audio recording will be performed.");
                return;
            }

            if (audioListeners.Length > 1)
            {
                Logger.LogWarning(
                    "Multiple AudioListeners found in the scene. First found AudioListener will be used for audio recording.");
            }

            _audioListener = audioListeners[0];
            StartRecordingAudio(ctx);
        }

        protected override void OnStopRecording(RecorderContext ctx)
        {
            base.OnStopRecording(ctx);

            if (_audioListener == null)
                return;

            StopRecordingAudio(ctx);
        }

        private void StartRecordingAudio(RecorderContext ctx)
        {
            var audioFilePath = GenerateAudioFilePath(Application.persistentDataPath, ctx);

            var directoryPath = Path.GetDirectoryName(audioFilePath);

            if (directoryPath != null && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            _channelCount = GetChannelCount(AudioSettings.speakerMode);
            _outputSampleRate = AudioSettings.outputSampleRate;
            var waveFormat = new WaveFormat(_outputSampleRate, _channelCount);
            _audioFileWriter = new WaveFileWriter(audioFilePath, waveFormat);

            var recorderScript = _audioListener.gameObject.AddComponent<AudioListenerRecorderScript>();
            recorderScript.Context = ctx;
            recorderScript.OnNewAudioSamples += OnNewAudioSamples;

            _audioRecordingThread = new Thread(() => RecordAudioLoop(ctx))
            {
                Name = "AudioRecordingThread"
            };
            _audioRecordingThread.Start();
        }

        private void StopRecordingAudio(RecorderContext ctx)
        {
            Object.Destroy(_audioListener.gameObject.GetComponent<AudioListener>());
            _audioRecordingThread?.Join();
            _audioRecordingThread = null;
            _audioFileWriter?.Dispose();
            _audioFileWriter = null;
        }

        private void RecordAudioLoop(RecorderContext ctx)
        {
            while (ctx.IsRecording || _audioSamplesQueue.Count > 0)
            {
                if (!_audioSamplesQueue.TryTake(out var audioSamples, 100)) continue;
                var samples = audioSamples.Samples;
                var channelCount = audioSamples.ChannelCount;
                var samplesTimestampSeconds = audioSamples.Timestamp / 1_000_000_000.0;
                var samplesChunkDurationSeconds = samples.Length / (double)_outputSampleRate / channelCount;
                var audioFileDurationSeconds = _audioFileWriter.TotalTime.TotalSeconds;

                // Samples are timestamped before the duration of the audio file, skipping.
                if (samplesTimestampSeconds < audioFileDurationSeconds)
                    continue;

                // Ensure recorder time and audio dsp time are in sync by inserting silence if needed
                if (samplesTimestampSeconds - samplesChunkDurationSeconds - audioFileDurationSeconds >
                    TriggerAudioSyncThresholdSeconds)
                {
                    var silenceDurationSeconds = samplesTimestampSeconds - audioFileDurationSeconds;
                    var silenceSamplesCount = (int)(silenceDurationSeconds * _outputSampleRate * _channelCount);
                    var silenceSamples = new float[silenceSamplesCount];
                    _audioFileWriter.WriteSamples(silenceSamples, 0, silenceSamples.Length);

                    if (_settings.LogSilenceInsertion)
                    {
                        Logger.Log(
                            $"Inserted {silenceDurationSeconds * 1000}ms of silence to audio recording to sync with recorder time.");
                    }
                }

                if (channelCount == _channelCount)
                {
                    _audioFileWriter.WriteSamples(samples, 0, samples.Length);
                }
                else
                {
                    Logger.LogWarning(
                        $"Channel count mismatch. Expected {_channelCount} channels, but received {channelCount} channels. Resampling audio samples to match expected channel count.");
                    // TODO: copy channels to match expected channel count
                    // var stride = channelCount * _outputSampleRate;
                    // var expectedStride = _channelCount * _outputSampleRate;
                    //
                    // for (var i = 0; i < samples.Length; i += stride)
                    // {
                    //     _audioFileWriter.WriteSamples(samples, 0, expectedStride);
                    // }
                }
            }
        }

        private void OnNewAudioSamples(AudioSamples audioSamples)
        {
            _audioSamplesQueue.Add(audioSamples);
        }

        private static string GenerateAudioFilePath(string outputDir, RecorderContext ctx)
        {
            var invalidChars = Path.GetInvalidFileNameChars().ToList();
            invalidChars.Add(' ');

            var name = ctx.CurrentRecord.Metadata.Name;
            var safeName = new string(name.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());

            var formattedDateTime = ctx.CurrentRecord.Metadata.StartTime.ToString("yyyy-MM-ddTHH-mm-sszz");
            var filenameBase = $"{safeName}_{formattedDateTime}";
            const string fileExtension = ".wav";

            var i = 0;
            string filePath;

            // Add suffix to filename if file already exists
            do
            {
                var suffix = i == 0 ? "" : "_" + i;
                filePath = Path.Join(outputDir, filenameBase + suffix + fileExtension);
                ++i;
            } while (File.Exists(filePath));

            return filePath;
        }

        private static int GetChannelCount(AudioSpeakerMode speakerMode)
        {
            // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
            return speakerMode switch
            {
                AudioSpeakerMode.Mono => 1,
                AudioSpeakerMode.Stereo => 2,
                AudioSpeakerMode.Quad => 4,
                AudioSpeakerMode.Surround => 5,
                AudioSpeakerMode.Mode5point1 => 6,
                AudioSpeakerMode.Mode7point1 => 8,
                AudioSpeakerMode.Prologic => 2,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}