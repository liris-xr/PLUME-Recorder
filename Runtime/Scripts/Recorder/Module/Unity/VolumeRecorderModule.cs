using System.Collections.Generic;
using PLUME.Sample.Unity;
using UnityEngine;
using UnityEngine.Rendering;

namespace PLUME.URP
{
    public class VolumeRecorderModule : RecorderModule,
        IStartRecordingObjectEventReceiver,
        IStopRecordingObjectEventReceiver
    {
        private readonly Dictionary<int, Volume> _recordedVolumes = new();
        private readonly Dictionary<int, bool> _lastEnabled = new();

        public void OnStartRecordingObject(Object obj)
        {
            if (obj is Volume volume && !_recordedVolumes.ContainsKey(volume.GetInstanceID()))
            {
                var volumeInstanceId = volume.GetInstanceID();
                _recordedVolumes.Add(volumeInstanceId, volume);
                _lastEnabled.Add(volumeInstanceId, volume.enabled);
                RecordCreation(volume);
            }
        }

        public void OnStopRecordingObject(int objectInstanceId)
        {
            if (_recordedVolumes.ContainsKey(objectInstanceId))
            {
                RecordDestruction(objectInstanceId);
                RemoveFromCache(objectInstanceId);
            }
        }

        private void RemoveFromCache(int volumeInstanceId)
        {
            _recordedVolumes.Remove(volumeInstanceId);
            _lastEnabled.Remove(volumeInstanceId);
        }

        public void FixedUpdate()
        {
            var nullVolumesInstanceIds = new List<int>();
            
            foreach (var (volumeInstanceId, volume) in _recordedVolumes)
            {
                if (volume == null)
                {
                    nullVolumesInstanceIds.Add(volumeInstanceId);
                    continue;
                }
                
                if (_lastEnabled[volumeInstanceId] != volume.enabled)
                {
                    _lastEnabled[volumeInstanceId] = volume.enabled;

                    var volumeUpdateEnabled = new VolumeUpdateEnabled
                    {
                        Id = volume.ToIdentifierPayload(),
                        Enabled = volume.enabled
                    };

                    recorder.RecordSampleStamped(volumeUpdateEnabled);
                }
            }

            foreach (var nullVolumeInstanceId in nullVolumesInstanceIds)
            {
                RecordDestruction(nullVolumeInstanceId);
                RemoveFromCache(nullVolumeInstanceId);
            }
        }
        
        private void RecordCreation(Volume volume)
        {
            var identifier = volume.ToIdentifierPayload();
            
            var volumeCreate = new VolumeCreate { Id = identifier };
            var volumeUpdate = new VolumeUpdate
            {
                Id = identifier,
                IsGlobal = volume.isGlobal,
                BlendDistance = volume.blendDistance,
                Weight = volume.weight,
                Priority = volume.priority,
                SharedProfile = volume.sharedProfile == null ? null : volume.sharedProfile.ToAssetIdentifierPayload()
            };
            
            var volumeUpdateEnabled = new VolumeUpdateEnabled
            {
                Id = identifier,
                Enabled = volume.enabled
            };
            
            recorder.RecordSampleStamped(volumeCreate);
            recorder.RecordSampleStamped(volumeUpdate);
            recorder.RecordSampleStamped(volumeUpdateEnabled);
        }

        private void RecordDestruction(int volumeInstanceId)
        {
            var volumeDestroy = new ComponentDestroy
                { Id = new ComponentDestroyIdentifier { Id = volumeInstanceId.ToString() } };
            recorder.RecordSampleStamped(volumeDestroy);
        }

        protected override void ResetCache()
        {
            _recordedVolumes.Clear();
            _lastEnabled.Clear();
        }
    }
}