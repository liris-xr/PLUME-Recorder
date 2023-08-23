using System.Collections.Generic;
using System.Linq;
using PLUME.Sample.Unity;
using UnityEngine;

namespace PLUME
{
    [DisallowMultipleComponent]
    public class MeshRendererRecorderModule : RecorderModule,
        IStartRecordingObjectEventReceiver,
        IStopRecordingObjectEventReceiver,
        IStartRecordingEventReceiver,
        IStopRecordingEventReceiver
    {
        private readonly Dictionary<int, MeshRenderer> _recordedMeshRenderers = new();
        private readonly Dictionary<int, bool> _lastEnabled = new();

        public void FixedUpdate()
        {
            if (_recordedMeshRenderers.Count == 0)
                return;

            var nullMeshRendererInstanceIds = new List<int>();

            foreach (var (meshRendererInstanceId, meshRenderer) in _recordedMeshRenderers)
            {
                if (meshRenderer == null)
                {
                    nullMeshRendererInstanceIds.Add(meshRendererInstanceId);
                    continue;
                }

                if (_lastEnabled[meshRendererInstanceId] != meshRenderer.enabled)
                {
                    _lastEnabled[meshRendererInstanceId] = meshRenderer.enabled;

                    var meshRendererUpdateEnabled = new MeshRendererUpdateEnabled()
                    {
                        Id = meshRenderer.ToIdentifierPayload(),
                        Enabled = meshRenderer.enabled
                    };

                    recorder.RecordSample(meshRendererUpdateEnabled);
                }
            }

            foreach (var meshRendererInstanceId in nullMeshRendererInstanceIds)
            {
                RecordDestruction(meshRendererInstanceId);
                RemoveFromCache(meshRendererInstanceId);
            }
        }

        public new void OnStartRecording()
        {
            base.OnStartRecording();
            RendererEvents.OnSetInstanceMaterials += (r, _) => OnUpdateMaterials(r);
            RendererEvents.OnSetInstanceMaterial += (r, _) => OnUpdateMaterials(r);
            RendererEvents.OnSetSharedMaterials += (r, _) => OnUpdateMaterials(r);
            RendererEvents.OnSetSharedMaterial += (r, _) => OnUpdateMaterials(r);
        }

        protected override void ResetCache()
        {
            _recordedMeshRenderers.Clear();
            _lastEnabled.Clear();
        }

        public void OnStopRecording()
        {
            RendererEvents.OnSetInstanceMaterials -= (r, _) => OnUpdateMaterials(r);
            RendererEvents.OnSetInstanceMaterial -= (r, _) => OnUpdateMaterials(r);
            RendererEvents.OnSetSharedMaterials -= (r, _) => OnUpdateMaterials(r);
            RendererEvents.OnSetSharedMaterial -= (r, _) => OnUpdateMaterials(r);
        }

        private void OnUpdateMaterials(Renderer r)
        {
            if (r is MeshRenderer meshRenderer && _recordedMeshRenderers.ContainsKey(r.GetInstanceID()))
            {
                var meshRendererUpdateMaterials = new MeshRendererUpdateMaterials
                {
                    Id = meshRenderer.ToIdentifierPayload(),
                    MaterialsIds = {meshRenderer.sharedMaterials.Select(m => m.ToAssetIdentifierPayload())}
                };

                recorder.RecordSample(meshRendererUpdateMaterials);
            }
        }

        public void OnStartRecordingObject(Object obj)
        {
            if (obj is MeshRenderer meshRenderer && !_recordedMeshRenderers.ContainsKey(meshRenderer.GetInstanceID()))
            {
                var meshRendererInstanceId = meshRenderer.GetInstanceID();
                _recordedMeshRenderers.Add(meshRendererInstanceId, meshRenderer);
                _lastEnabled.Add(meshRendererInstanceId, meshRenderer.enabled);
                RecordCreation(meshRenderer);
            }
        }

        public void OnStopRecordingObject(int objectInstanceId)
        {
            if (_recordedMeshRenderers.ContainsKey(objectInstanceId))
            {
                RecordDestruction(objectInstanceId);
                RemoveFromCache(objectInstanceId);
            }
        }

        private void RecordCreation(MeshRenderer meshRenderer)
        {
            var meshRendererCreate = new MeshRendererCreate {Id = meshRenderer.ToIdentifierPayload()};

            var meshRendererUpdateInstanceMaterials = new MeshRendererUpdateMaterials
            {
                Id = meshRenderer.ToIdentifierPayload(),
                MaterialsIds = {meshRenderer.sharedMaterials.Select(m => m.ToAssetIdentifierPayload())}
            };

            var meshRendererUpdateEnabled = new MeshRendererUpdateEnabled()
            {
                Id = meshRenderer.ToIdentifierPayload(),
                Enabled = meshRenderer.enabled
            };

            recorder.RecordSample(meshRendererCreate);
            recorder.RecordSample(meshRendererUpdateInstanceMaterials);
            recorder.RecordSample(meshRendererUpdateEnabled);
        }

        private void RemoveFromCache(int meshRendererInstanceId)
        {
            _recordedMeshRenderers.Remove(meshRendererInstanceId);
            _lastEnabled.Remove(meshRendererInstanceId);
        }

        private void RecordDestruction(int meshRendererInstanceId)
        {
            var meshRendererDestroy = new ComponentDestroy
                {Id = new ComponentDestroyIdentifier {Id = meshRendererInstanceId.ToString()}};
            recorder.RecordSample(meshRendererDestroy);
        }
    }
}