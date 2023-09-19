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
        private readonly Dictionary<int, SkinnedMeshRenderer> _recordedSkinnedMeshRenderers = new();
        private readonly Dictionary<int, bool> _lastEnabled = new();

        public void OnStartRecordingObject(Object obj)
        {
            if (obj is SkinnedMeshRenderer skinnedMeshRenderer &&
                !_recordedSkinnedMeshRenderers.ContainsKey(skinnedMeshRenderer.GetInstanceID()))
            {
                var skinnedMeshRendererInstanceId = skinnedMeshRenderer.GetInstanceID();
                _recordedSkinnedMeshRenderers.Add(skinnedMeshRendererInstanceId, skinnedMeshRenderer);
                _lastEnabled.Add(skinnedMeshRendererInstanceId, skinnedMeshRenderer.enabled);
                RecordCreation(skinnedMeshRenderer);
            }
        }

        public void OnStopRecordingObject(int objectInstanceId)
        {
            if (_recordedSkinnedMeshRenderers.ContainsKey(objectInstanceId))
            {
                RecordDestruction(objectInstanceId);
                RemoveFromCache(objectInstanceId);
            }
        }

        private void RemoveFromCache(int skinnedMeshRendererInstanceId)
        {
            _recordedSkinnedMeshRenderers.Remove(skinnedMeshRendererInstanceId);
            _lastEnabled.Remove(skinnedMeshRendererInstanceId);
        }

        protected override void ResetCache()
        {
            _recordedSkinnedMeshRenderers.Clear();
            _lastEnabled.Clear();
        }

        public void FixedUpdate()
        {
            var nullSkinnedMeshRendererInstanceIds = new List<int>();

            foreach (var (skinnedMeshRendererInstanceId, skinnedMeshRenderer) in _recordedSkinnedMeshRenderers)
            {
                if (skinnedMeshRenderer == null)
                {
                    nullSkinnedMeshRendererInstanceIds.Add(skinnedMeshRendererInstanceId);
                    continue;
                }

                if (_lastEnabled[skinnedMeshRendererInstanceId] != skinnedMeshRenderer.enabled)
                {
                    _lastEnabled[skinnedMeshRendererInstanceId] = skinnedMeshRenderer.enabled;

                    var skinnedMeshRendererUpdateEnabled = new SkinnedMeshRendererUpdateEnabled
                    {
                        Id = skinnedMeshRenderer.ToIdentifierPayload(),
                        Enabled = skinnedMeshRenderer.enabled
                    };

                    recorder.RecordSample(skinnedMeshRendererUpdateEnabled);
                }
            }

            foreach (var skinnedMeshRendererInstanceId in nullSkinnedMeshRendererInstanceIds)
            {
                RecordDestruction(skinnedMeshRendererInstanceId);
                RemoveFromCache(skinnedMeshRendererInstanceId);
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
                BonesIds = {skinnedMeshRenderer.bones.Select(b => b.ToIdentifierPayload())}
            };

            var skinnedMeshRendererUpdateMaterials = new SkinnedMeshRendererUpdateMaterials
            {
                Id = skinnedMeshRendererIdentifier,
                MaterialsIds = {skinnedMeshRenderer.sharedMaterials.Select(m => m.ToAssetIdentifierPayload())}
            };

            var skinnedMeshRendererUpdateEnabled = new SkinnedMeshRendererUpdateEnabled
            {
                Id = skinnedMeshRenderer.ToIdentifierPayload(),
                Enabled = skinnedMeshRenderer.enabled
            };
            
            var skinnedMeshRendererUpdateLightmap = new SkinnedMeshRendererUpdateLightmap
            {
                Id = skinnedMeshRenderer.ToIdentifierPayload(),
                LightmapIndex = skinnedMeshRenderer.lightmapIndex,
                LightmapScaleOffset = skinnedMeshRenderer.lightmapScaleOffset.ToPayload(),
                RealtimeLightmapIndex = skinnedMeshRenderer.realtimeLightmapIndex,
                RealtimeLightmapScaleOffset = skinnedMeshRenderer.realtimeLightmapScaleOffset.ToPayload()
            };

            recorder.RecordSample(skinnedMeshRendererCreate);
            recorder.RecordSample(skinnedMeshRendererUpdateSharedMesh);
            recorder.RecordSample(skinnedMeshRendererUpdateBones);
            recorder.RecordSample(skinnedMeshRendererUpdateMaterials);
            recorder.RecordSample(skinnedMeshRendererUpdateEnabled);
            recorder.RecordSample(skinnedMeshRendererUpdateLightmap);
        }

        private void RecordDestruction(int skinnedMeshRendererInstanceId)
        {
            var skinnedMeshRendererDestroy = new ComponentDestroy
                {Id = new ComponentDestroyIdentifier {Id = skinnedMeshRendererInstanceId.ToString()}};
            recorder.RecordSample(skinnedMeshRendererDestroy);
        }
    }
}