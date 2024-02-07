using System.Collections.Generic;
using PLUME.Sample.Unity;
using UnityEngine;

namespace PLUME.Recorder.Module.Unity
{
    public class TerrainColliderRecorderModule : RecorderModule, IStartRecordingObjectEventReceiver,
        IStopRecordingObjectEventReceiver
    {
        private readonly Dictionary<int, TerrainCollider> _recordedTerrainColliders = new();
        private readonly Dictionary<int, bool> _lastEnabled = new();


        protected override void ResetCache()
        {
            _recordedTerrainColliders.Clear();
            _lastEnabled.Clear();
        }

        public void OnStartRecordingObject(Object obj)
        {
            // ReSharper disable once LocalVariableHidesMember
            if (obj is TerrainCollider terrain && !_recordedTerrainColliders.ContainsKey(terrain.GetInstanceID()))
            {
                var terrainInstanceId = terrain.GetInstanceID();
                _recordedTerrainColliders.Add(terrainInstanceId, terrain);
                _lastEnabled.Add(terrainInstanceId, terrain.enabled);
                RecordCreation(terrain);
            }
        }

        public void OnStopRecordingObject(int objectInstanceId)
        {
            if (_recordedTerrainColliders.ContainsKey(objectInstanceId))
            {
                RecordDestruction(objectInstanceId);
                RemoveFromCache(objectInstanceId);
            }
        }

        private void RemoveFromCache(int terrainInstanceId)
        {
            _recordedTerrainColliders.Remove(terrainInstanceId);
            _lastEnabled.Remove(terrainInstanceId);
        }

        public void FixedUpdate()
        {
            var nullTerrainCollidersInstanceIds = new List<int>();

            foreach (var (terrainColliderInstanceId, terrainCollider) in _recordedTerrainColliders)
            {
                if (terrainCollider == null)
                {
                    nullTerrainCollidersInstanceIds.Add(terrainColliderInstanceId);
                    continue;
                }

                if (_lastEnabled[terrainColliderInstanceId] != terrainCollider.enabled)
                {
                    _lastEnabled[terrainColliderInstanceId] = terrainCollider.enabled;

                    var terrainColliderUpdateEnabled = new TerrainColliderUpdateEnabled
                    {
                        Id = terrainCollider.ToIdentifierPayload(),
                        Enabled = terrainCollider.enabled
                    };

                    recorder.RecordSampleStamped(terrainColliderUpdateEnabled);
                }
            }

            foreach (var nullTerrainColliderInstanceId in nullTerrainCollidersInstanceIds)
            {
                RecordDestruction(nullTerrainColliderInstanceId);
                RemoveFromCache(nullTerrainColliderInstanceId);
            }
        }

        private void RecordCreation(TerrainCollider terrainCollider)
        {
            var terrainColliderIdentifier = terrainCollider.ToIdentifierPayload();

            var terrainColliderCreate = new TerrainColliderCreate { Id = terrainColliderIdentifier };

            var terrainColliderUpdate = new TerrainColliderUpdate
            {
                Id = terrainColliderIdentifier,
                TerrainDataId = terrainCollider.terrainData == null ? null : terrainCollider.terrainData.ToAssetIdentifierPayload(),
                MaterialId = terrainCollider.sharedMaterial == null ? null : terrainCollider.sharedMaterial.ToAssetIdentifierPayload()
            };

            var terrainColliderUpdateEnabled = new TerrainColliderUpdateEnabled
            {
                Id = terrainCollider.ToIdentifierPayload(),
                Enabled = terrainCollider.enabled
            };

            recorder.RecordSampleStamped(terrainColliderCreate);
            recorder.RecordSampleStamped(terrainColliderUpdate);
            recorder.RecordSampleStamped(terrainColliderUpdateEnabled);
        }

        private void RecordDestruction(int terrainColliderInstanceId)
        {
            var terrainColliderDestroy = new ComponentDestroy
                { Id = new ComponentDestroyIdentifier { Id = terrainColliderInstanceId.ToString() } };
            recorder.RecordSampleStamped(terrainColliderDestroy);
        }
    }
}