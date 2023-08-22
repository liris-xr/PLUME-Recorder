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
        private readonly HashSet<MeshRenderer> _recordedMeshRenderers = new();
        private readonly Dictionary<MeshRenderer, bool> _lastEnabled = new();

        public void FixedUpdate()
        {
            if (_recordedMeshRenderers.Count == 0)
                return;

            foreach (var meshRenderer in _recordedMeshRenderers)
            {
                if (_lastEnabled[meshRenderer] != meshRenderer.enabled)
                {
                    _lastEnabled[meshRenderer] = meshRenderer.enabled;

                    var meshRendererUpdateEnabled = new MeshRendererUpdateEnabled()
                    {
                        Id = meshRenderer.ToIdentifierPayload(),
                        Enabled = meshRenderer.enabled
                    };

                    recorder.RecordSample(meshRendererUpdateEnabled);
                }
            }
        }

        public void OnStartRecording()
        {
            RendererEvents.OnSetInstanceMaterials += (r, _) => OnUpdateMaterials(r);
            RendererEvents.OnSetInstanceMaterial += (r, _) => OnUpdateMaterials(r);
            RendererEvents.OnSetSharedMaterials += (r, _) => OnUpdateMaterials(r);
            RendererEvents.OnSetSharedMaterial += (r, _) => OnUpdateMaterials(r);
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
            if (r is MeshRenderer meshRenderer && _recordedMeshRenderers.Contains(r))
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
            if (obj is MeshRenderer meshRenderer && !_recordedMeshRenderers.Contains(meshRenderer))
            {
                _recordedMeshRenderers.Add(meshRenderer);
                _lastEnabled.Add(meshRenderer, meshRenderer.enabled);
                RecordCreation(meshRenderer);
            }
        }

        public void OnStopRecordingObject(Object obj)
        {
            if (obj is MeshRenderer meshRenderer && _recordedMeshRenderers.Contains(meshRenderer))
            {
                _recordedMeshRenderers.Remove(meshRenderer);
                _lastEnabled.Remove(meshRenderer);
                RecordDestruction(meshRenderer);
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

        private void RecordDestruction(MeshRenderer meshRenderer)
        {
            var meshRendererDestroy = new MeshRendererDestroy {Id = meshRenderer.ToIdentifierPayload()};
            recorder.RecordSample(meshRendererDestroy);
        }
    }
}