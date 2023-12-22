using System.Collections.Generic;
using PLUME.Sample;
using PLUME.Sample.Unity;
using UnityEngine;

namespace PLUME {

    [DisallowMultipleComponent]
    public class AudioSourceRecorderModule : RecorderModule,
        IStartRecordingObjectEventReceiver,
        IStopRecordingObjectEventReceiver
    {
        private readonly Dictionary<int, AudioSource> _recordedAudioSources = new();
        private readonly Dictionary<int, bool> _lastPlayingStatus = new();
        private readonly Dictionary<int, int> _lastTimeSamples = new();
        private readonly Dictionary<int, AudioClip> _lastClip = new();

        public void OnStartRecordingObject(Object obj)
        {
            if (obj is AudioSource audioSource && !_recordedAudioSources.ContainsKey(audioSource.GetInstanceID()))
            {
                var instanceId = audioSource.GetInstanceID();
                _recordedAudioSources.Add(instanceId, audioSource);
                RecordCreation(audioSource);
                _lastPlayingStatus.Add(instanceId, audioSource.isPlaying);
                _lastTimeSamples.Add(instanceId, audioSource.timeSamples);
                _lastClip.Add(instanceId, audioSource.clip);
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

        public void FixedUpdate()
        {
            var nullAudioSourceInstanceIds = new List<int>();
            
            var recordedSamples = new List<UnpackedSample>();
            
            foreach (var (audioSourceId, audioSource) in _recordedAudioSources)
            {
                if (audioSource == null)
                {
                    nullAudioSourceInstanceIds.Add(audioSourceId);
                    continue;
                }

                if (_lastPlayingStatus[audioSourceId] != audioSource.isPlaying)
                {
                    var updatePlayStatus = new AudioSourceUpdatePlayStatus
                    {
                        Id = audioSource.ToIdentifierPayload(),
                        IsPlaying = audioSource.isPlaying
                    };
                    
                    recordedSamples.Add(recorder.GetUnpackedSampleStamped(updatePlayStatus));
                    _lastPlayingStatus[audioSourceId] = audioSource.isPlaying;
                }

                if (_lastTimeSamples[audioSourceId] != audioSource.timeSamples)
                {
                    var updateTime = new AudioSourceUpdateTime
                    {
                        Id = audioSource.ToIdentifierPayload(),
                        TimeSamples = audioSource.timeSamples
                    };
                    
                    recordedSamples.Add(recorder.GetUnpackedSampleStamped(updateTime));
                    _lastTimeSamples[audioSourceId] = audioSource.timeSamples;
                }

                if (_lastClip[audioSourceId] != audioSource.clip)
                {
                    var updateClip = new AudioSourceUpdateClip
                    {
                        Id = audioSource.ToIdentifierPayload(),
                        AudioClipId = audioSource.clip.ToAssetIdentifierPayload()
                    };
                    
                    recordedSamples.Add(recorder.GetUnpackedSampleStamped(updateClip));
                    _lastClip[audioSourceId] = audioSource.clip;
                }
                
                // TODO: Add support for other changes
            }
            
            recorder.RecordUnpackedSamples(recordedSamples);
            
            foreach (var nullTransformInstanceId in nullAudioSourceInstanceIds)
            {
                RecordDestruction(nullTransformInstanceId);
                RemoveFromCache(nullTransformInstanceId);
            }
        }

        private void RecordCreation(AudioSource audioSource)
        {
            var identifier = audioSource.ToIdentifierPayload();
            var audioSourceCreate = new AudioSourceCreate { Id = identifier };
            var updateClip = new AudioSourceUpdateClip
            {
                Id = identifier,
                AudioClipId = audioSource.clip.ToAssetIdentifierPayload()
            };
            var updateMixer = new AudioSourceUpdateMixer
            {
                Id = identifier,
                AudioMixerGroupId = audioSource.outputAudioMixerGroup.ToAssetIdentifierPayload()
            };
            var updatePlayStatus = new AudioSourceUpdatePlayStatus
            {
                Id = identifier,
                IsPlaying = audioSource.isPlaying
            };
            var updateTime = new AudioSourceUpdateTime
            {
                Id = identifier,
                TimeSamples = audioSource.timeSamples
            };
            var updateMute = new AudioSourceUpdateMute
            {
                Id = identifier,
                Mute = audioSource.mute
            };
            var updateBypass = new AudioSourceUpdateBypass
            {
                Id = identifier,
                BypassEffects = audioSource.bypassEffects,
                BypassListenerEffects = audioSource.bypassListenerEffects,
                BypassReverbZones = audioSource.bypassReverbZones
            };
            var updatePriority = new AudioSourceUpdatePriority
            {
                Id = identifier,
                Priority = audioSource.priority
            };
            var updateVolume = new AudioSourceUpdateVolume
            {
                Id = identifier,
                Volume = audioSource.volume
            };
            var updatePitch = new AudioSourceUpdatePitch
            {
                Id = identifier,
                Pitch = audioSource.pitch
            };
            var updatePanStereo = new AudioSourceUpdateStereoPan
            {
                Id = identifier,
                StereoPan = audioSource.panStereo
            };
            var updateSpatialBlend = new AudioSourceUpdateSpatialBlend
            {
                Id = identifier,
                SpatialBlend = audioSource.GetCustomCurve(AudioSourceCurveType.SpatialBlend).ToPayload()
            };
            var updateReverbZoneMix = new AudioSourceUpdateReverbZoneMix
            {
                Id = identifier,
                ReverbZoneMix = audioSource.GetCustomCurve(AudioSourceCurveType.ReverbZoneMix).ToPayload()
            };
            var updateDopplerLevel = new AudioSourceUpdateDopplerLevel
            {
                Id = identifier,
                DopplerLevel = audioSource.dopplerLevel
            };
            var updateSpread = new AudioSourceUpdateSpread
            {
                Id = identifier,
                Spread = audioSource.GetCustomCurve(AudioSourceCurveType.Spread).ToPayload()
            };
            var updateVolumeRolloff = new AudioSourceUpdateVolumeRolloff
            {
                Id = identifier,
                VolumeRolloff = audioSource.GetCustomCurve(AudioSourceCurveType.CustomRolloff).ToPayload()
            };

            recorder.RecordSampleStamped(audioSourceCreate);
            recorder.RecordSampleStamped(updateClip);
            recorder.RecordSampleStamped(updateMixer);
            recorder.RecordSampleStamped(updatePlayStatus);
            recorder.RecordSampleStamped(updateTime);
            recorder.RecordSampleStamped(updateMute);
            recorder.RecordSampleStamped(updateBypass);
            recorder.RecordSampleStamped(updatePriority);
            recorder.RecordSampleStamped(updateVolume);
            recorder.RecordSampleStamped(updatePitch);
            recorder.RecordSampleStamped(updatePanStereo);
            recorder.RecordSampleStamped(updateSpatialBlend);
            recorder.RecordSampleStamped(updateReverbZoneMix);
            recorder.RecordSampleStamped(updateDopplerLevel);
            recorder.RecordSampleStamped(updateSpread);
            recorder.RecordSampleStamped(updateVolumeRolloff);
        }

        private void RecordDestruction(int cameraInstanceId)
        {
            var audioSourceDestroy = new ComponentDestroy
                { Id = new ComponentDestroyIdentifier { Id = cameraInstanceId.ToString() } };
            recorder.RecordSampleStamped(audioSourceDestroy);
        }
        
        private void RemoveFromCache(int audioSourceInstanceId)
        {
            _recordedAudioSources.Remove(audioSourceInstanceId);
            _lastPlayingStatus.Remove(audioSourceInstanceId);
            _lastTimeSamples.Remove(audioSourceInstanceId);
            _lastClip.Remove(audioSourceInstanceId);
        }

        protected override void ResetCache()
        {
            _recordedAudioSources.Clear();
            _lastPlayingStatus.Clear();
            _lastTimeSamples.Clear();
            _lastClip.Clear();
        }
    }
}