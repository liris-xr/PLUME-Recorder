using System.Collections.Generic;
using System.Linq;
using PLUME.Sample.Unity;
using UnityEngine;

namespace PLUME
{
    [DisallowMultipleComponent]
    public class SkinnedMeshRendererRecorderModule : RecorderModule,
        IStartRecordingObjectEventReceiver,
        IStopRecordingObjectEventReceiver
    {
        private readonly HashSet<SkinnedMeshRenderer> _recordedSkinnedMeshRenderers = new();
        private readonly Dictionary<SkinnedMeshRenderer, bool> _lastEnabled = new();

        public void OnStartRecordingObject(Object obj)
        {
            if (obj is SkinnedMeshRenderer skinnedMeshRenderer &&
                !_recordedSkinnedMeshRenderers.Contains(skinnedMeshRenderer))
            {
                _recordedSkinnedMeshRenderers.Add(skinnedMeshRenderer);
                _lastEnabled.Add(skinnedMeshRenderer, skinnedMeshRenderer.enabled);
                RecordCreation(skinnedMeshRenderer);
            }
        }

        public void OnStopRecordingObject(Object obj)
        {
            if (obj is SkinnedMeshRenderer skinnedMeshRenderer &&
                _recordedSkinnedMeshRenderers.Contains(skinnedMeshRenderer))
            {
                _recordedSkinnedMeshRenderers.Remove(skinnedMeshRenderer);
                _lastEnabled.Remove(skinnedMeshRenderer);
                RecordDestruction(skinnedMeshRenderer);
            }
        }

        public void FixedUpdate()
        {
            foreach (var skinnedMeshRenderer in _recordedSkinnedMeshRenderers)
            {
                if (_lastEnabled[skinnedMeshRenderer] != skinnedMeshRenderer.enabled)
                {
                    _lastEnabled[skinnedMeshRenderer] = skinnedMeshRenderer.enabled;

                    var skinnedMeshRendererUpdateEnabled = new SkinnedMeshRendererUpdateEnabled
                    {
                        Id = skinnedMeshRenderer.ToIdentifierPayload(),
                        Enabled = skinnedMeshRenderer.enabled
                    };

                    recorder.RecordSample(skinnedMeshRendererUpdateEnabled);
                }
            }
        }

        private void RecordCreation(SkinnedMeshRenderer skinnedMeshRenderer)
        {
            var skinnedMeshRendererIdentifier = skinnedMeshRenderer.ToIdentifierPayload();
            var skinnedMeshRendererCreate = new SkinnedMeshRendererCreate {Id = skinnedMeshRendererIdentifier};

            var skinnedMeshRendererUpdateSharedMesh = new SkinnedMeshRendererUpdateMesh
            {
                Id = skinnedMeshRendererIdentifier,
                MeshId = skinnedMeshRenderer.sharedMesh == null
                    ? null
                    : skinnedMeshRenderer.sharedMesh.ToAssetIdentifierPayload()
            };
            
            var skinnedMeshRendererUpdateBones = new SkinnedMeshRendererUpdateBones
            {
                Id = skinnedMeshRendererIdentifier,
                RootBoneId = skinnedMeshRenderer.rootBone == null
                    ? null
                    : skinnedMeshRenderer.rootBone.ToIdentifierPayload(),
                BonesIds = { skinnedMeshRenderer.bones.Select(b => b.ToIdentifierPayload()) }
            };
            
            var skinnedMeshRendererUpdateMaterials = new SkinnedMeshRendererUpdateMaterials
            {
                Id = skinnedMeshRendererIdentifier,
                MaterialsIds = { skinnedMeshRenderer.sharedMaterials.Select(m => m.ToAssetIdentifierPayload()) }
            };

            var skinnedMeshRendererUpdateEnabled = new SkinnedMeshRendererUpdateEnabled
            {
                Id = skinnedMeshRenderer.ToIdentifierPayload(),
                Enabled = skinnedMeshRenderer.enabled
            };

            recorder.RecordSample(skinnedMeshRendererCreate);
            recorder.RecordSample(skinnedMeshRendererUpdateSharedMesh);
            recorder.RecordSample(skinnedMeshRendererUpdateBones);
            recorder.RecordSample(skinnedMeshRendererUpdateMaterials);
            recorder.RecordSample(skinnedMeshRendererUpdateEnabled);
        }

        private void RecordDestruction(SkinnedMeshRenderer skinnedMeshRenderer)
        {
            var skinnedMeshRendererDestroy = new SkinnedMeshRendererDestroy
                {Id = skinnedMeshRenderer.ToIdentifierPayload()};
            recorder.RecordSample(skinnedMeshRendererDestroy);
        }
    }
}