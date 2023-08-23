using System.Collections.Generic;
using PLUME.Sample.Unity;
using UnityEngine;

namespace PLUME
{
    [DisallowMultipleComponent]
    public class MeshFilterRecorderModule : RecorderModule,
        IStartRecordingObjectEventReceiver,
        IStopRecordingObjectEventReceiver
    {
        private readonly Dictionary<int, MeshFilter> _recordedMeshFilters = new();
        private readonly Dictionary<int, int> _lastMeshInstanceIds = new();

        protected override void ResetCache()
        {
            _recordedMeshFilters.Clear();
            _lastMeshInstanceIds.Clear();
        }

        public void FixedUpdate()
        {
            if (_recordedMeshFilters.Count == 0)
                return;

            var nullMeshFilterInstanceIds = new List<int>();

            foreach (var (meshFilterInstanceId, meshFilter) in _recordedMeshFilters)
            {
                if (meshFilter == null)
                {
                    nullMeshFilterInstanceIds.Add(meshFilterInstanceId);
                    continue;
                }

                var lastMeshInstanceId = _lastMeshInstanceIds.GetValueOrDefault(meshFilterInstanceId);

                var sharedMeshId = meshFilter.sharedMesh == null ? 0 : meshFilter.sharedMesh.GetInstanceID();
                var updateMesh = lastMeshInstanceId != sharedMeshId;

                if (updateMesh)
                {
                    var meshFilterUpdateSharedMesh = new MeshFilterUpdateMesh
                    {
                        Id = meshFilter.ToIdentifierPayload(),
                        MeshId = meshFilter.sharedMesh == null
                            ? null
                            : meshFilter.sharedMesh.ToAssetIdentifierPayload()
                    };
                    _lastMeshInstanceIds[meshFilterInstanceId] = sharedMeshId;
                    recorder.RecordSample(meshFilterUpdateSharedMesh);
                }
            }

            foreach (var nullMeshFilterInstanceId in nullMeshFilterInstanceIds)
            {
                RecordDestruction(nullMeshFilterInstanceId);
                RemoveFromCache(nullMeshFilterInstanceId);
            }
        }

        private void RemoveFromCache(int meshFilterInstanceId)
        {
            _recordedMeshFilters.Remove(meshFilterInstanceId);
            _lastMeshInstanceIds.Remove(meshFilterInstanceId);
        }

        public void OnStartRecordingObject(Object obj)
        {
            if (obj is MeshFilter meshFilter && !_recordedMeshFilters.ContainsKey(meshFilter.GetInstanceID()))
            {
                _recordedMeshFilters.Add(meshFilter.GetInstanceID(), meshFilter);
                RecordCreation(meshFilter);
            }
        }

        public void OnStopRecordingObject(int objectInstanceId)
        {
            if (_recordedMeshFilters.ContainsKey(objectInstanceId))
            {
                RecordDestruction(objectInstanceId);
                RemoveFromCache(objectInstanceId);
            }
        }

        private void RecordCreation(MeshFilter meshFilter)
        {
            var meshFilterCreate = new MeshFilterCreate {Id = meshFilter.ToIdentifierPayload()};
            recorder.RecordSample(meshFilterCreate);
        }

        private void RecordDestruction(int meshFilterInstanceId)
        {
            var meshFilterDestroy = new ComponentDestroy
                {Id = new ComponentDestroyIdentifier {Id = meshFilterInstanceId.ToString()}};
            recorder.RecordSample(meshFilterDestroy);
        }
    }
}