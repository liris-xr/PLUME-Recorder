using System;
using PLUME.Core.Recorder;
using UnityEngine;

namespace PLUME.Base.Module.Unity.Audio
{
    [DisallowMultipleComponent]
    public class AudioListenerRecorderScript : MonoBehaviour
    {
        private AudioListener _audioListener;

        public delegate void OnNewAudioSamplesDelegate(AudioSamples audioSamples);

        public event OnNewAudioSamplesDelegate OnNewAudioSamples = delegate { };

        [NonSerialized] public RecorderContext Context;

        public void Awake()
        {
            _audioListener = GetComponent<AudioListener>();

            if (_audioListener == null)
            {
                Debug.LogWarning("No AudioListener found in the game object.");
                Destroy(this);
            }
        }

        public void OnAudioFilterRead(float[] data, int channels)
        {
            if (Context == null || !Context.IsRecording)
                return;
            
            var currentRecord = Context.CurrentRecord;
            
            if (currentRecord == null)
                return;

            if (data.Length == 0)
                return;

            OnNewAudioSamples(new AudioSamples(currentRecord.Time, data, channels));
        }
    }
}