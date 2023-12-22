using System.Collections.Generic;
using PLUME.Sample.Unity;
using UnityEngine;
using UnityRuntimeGuid;

namespace PLUME {

    [DisallowMultipleComponent]
    public class AudioRecorderModule : RecorderModule,
        IStartRecordingObjectEventReceiver,
        IStopRecordingObjectEventReceiver
    {
        private readonly Dictionary<int, AudioSource> _recordedAudioSources = new();

        public void OnStartRecordingObject(Object obj)
        {
            if (obj is AudioSource audioSource && !_recordedAudioSources.ContainsKey(audioSource.GetInstanceID()))
            {
                _recordedAudioSources.Add(audioSource.GetInstanceID(), audioSource);
                RecordCreation(audioSource);
            }
        }

        public void OnStopRecordingObject(int objectInstanceId)
        {
            if (_recordedAudioSources.ContainsKey(objectInstanceId))
            {
                RecordDestruction(objectInstanceId);
                RemoveFromCache(objectInstanceId);
            }
        }

        private void RemoveFromCache(int cameraInstanceId)
        {
            _recordedAudioSources.Remove(cameraInstanceId);
        }

        private void RecordCreation(AudioSource audioSource)
        {
            var identifier = audioSource.ToIdentifierPayload();
            var audioSourceCreate = new AudioSourceCreate { Id = identifier };
            var audioSourceUpdate = new AudioSourceUpdate
            {
                Id = identifier,
            };

            recorder.RecordSampleStamped(audioSourceCreate);
            recorder.RecordSampleStamped(audioSourceUpdate);
        }

        private void RecordDestruction(int cameraInstanceId)
        {
            var audioSourceDestroy = new ComponentDestroy
                { Id = new ComponentDestroyIdentifier { Id = cameraInstanceId.ToString() } };
            recorder.RecordSampleStamped(audioSourceDestroy);
        }

        protected override void ResetCache()
        {
            _recordedAudioSources.Clear();
        }
    }
}