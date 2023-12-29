using System;
using System.IO;
using UnityEngine;

namespace PLUME
{
    [DisallowMultipleComponent]
    public class AudioListenerRecorderModule : RecorderModule,
        IStartRecordingEventReceiver,
        IStopRecordingEventReceiver
    {
        private string _audioFilePath;
        
        private WavWriter _audioFileWriter;
        private int _channelCount;
        private int _dspBufferSize;
        
        private float[] _samplesBuffer;
        private float[] _singleChannelSamplesBuffer;
        
        private double _dspTime;
        private double _lastDspTime;
        
        public new void OnStartRecording()
        {
            base.OnStartRecording();
            
            _audioFilePath = Path.ChangeExtension(Recorder.Instance.RecordFilePath, ".wav");
            var directoryPath = Path.GetDirectoryName(_audioFilePath);
            
            if(directoryPath != null && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            
            _audioFileWriter?.Dispose();

            var fs = new FileStream(_audioFilePath, FileMode.Create);
            
            _channelCount = GetChannelCount(AudioSettings.speakerMode);
            AudioSettings.GetDSPBufferSize(out _dspBufferSize, out _);
            
            _samplesBuffer = new float[_dspBufferSize * _channelCount];
            _singleChannelSamplesBuffer = new float[_dspBufferSize];
            
            _audioFileWriter = new WavWriter(fs, AudioSettings.outputSampleRate, _channelCount);
            
            _dspTime = AudioSettings.dspTime;
            _lastDspTime = _dspTime;
        }

        public void OnStopRecording()
        {
            _audioFileWriter.Close();
            Debug.Log("Wrote recorded audio at " + new FileInfo(_audioFilePath).FullName);
        }
        
        public void Update()
        {
            _lastDspTime = _dspTime;
            _dspTime = AudioSettings.dspTime;
            
            var dtDsp = _dspTime - _lastDspTime;

            // No DSP update thus no new audio samples, skipping.
            if (dtDsp <= 0) return;
            
            var totalBufferSize = Mathf.NextPowerOfTwo((int)(dtDsp * AudioSettings.outputSampleRate * _channelCount));
            var singleChannelBufferSize = Mathf.NextPowerOfTwo((int)(dtDsp * AudioSettings.outputSampleRate));

            if(totalBufferSize != _samplesBuffer.Length)
                _samplesBuffer = new float[totalBufferSize];
            if(singleChannelBufferSize != _singleChannelSamplesBuffer.Length)
                _singleChannelSamplesBuffer = new float[singleChannelBufferSize];

            for (var channelIdx = 0; channelIdx < _channelCount; ++channelIdx)
            {
                AudioListener.GetOutputData(_singleChannelSamplesBuffer, channelIdx);
                for (var i = 0; i < _singleChannelSamplesBuffer.Length; i++)
                {
                    _samplesBuffer[i * _channelCount + channelIdx] = _singleChannelSamplesBuffer[i];
                }
            }
            
            _audioFileWriter.WriteWaveData(_samplesBuffer);
        }
        
        protected override void ResetCache()
        {
            _samplesBuffer = null;
            _singleChannelSamplesBuffer = null;
            _audioFilePath = null;
            _audioFileWriter?.Dispose();
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