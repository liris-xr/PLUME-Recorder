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
        private readonly HashSet<MeshFilter> _recordedMeshFilters = new();
        private readonly Dictionary<MeshFilter, int> _lastMeshInstanceIds = new();

        public void Reset()
        {
            _lastMeshInstanceIds.Clear();
        }

        public void FixedUpdate()
        {
            if (_recordedMeshFilters.Count == 0)
                return;

            foreach (var meshFilter in _recordedMeshFilters)
            {
                var lastMeshInstanceId = _lastMeshInstanceIds.GetValueOrDefault(meshFilter);

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
                    _lastMeshInstanceIds[meshFilter] = sharedMeshId;
                    recorder.RecordSample(meshFilterUpdateSharedMesh);
                }
            }
        }

        public void OnStartRecordingObject(Object obj)
        {
            if (obj is MeshFilter meshFilter && !_recordedMeshFilters.Contains(meshFilter))
            {
                _recordedMeshFilters.Add(meshFilter);
                RecordCreation(meshFilter);
            }
        }

        public void OnStopRecordingObject(Object obj)
        {
            if (obj is MeshFilter meshFilter && _recordedMeshFilters.Contains(meshFilter))
            {
                _recordedMeshFilters.Remove(meshFilter);
                _lastMeshInstanceIds.Remove(meshFilter);
                RecordDestruction(meshFilter);
            }
        }

        private void RecordCreation(MeshFilter meshFilter)
        {
            var meshFilterCreate = new MeshFilterCreate {Id = meshFilter.ToIdentifierPayload()};
            recorder.RecordSample(meshFilterCreate);
        }

        private void RecordDestruction(MeshFilter meshFilter)
        {
            var meshFilterDestroy = new MeshFilterDestroy {Id = meshFilter.ToIdentifierPayload()};
            recorder.RecordSample(meshFilterDestroy);
        }
    }
}