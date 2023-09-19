using System.Collections.Generic;
using PLUME.Sample.Unity;
using UnityEngine;
using UnityEngine.Rendering;

namespace PLUME
{
    [DisallowMultipleComponent]
    public class ReflectionProbeRecorderModule : RecorderModule,
        IStartRecordingObjectEventReceiver,
        IStopRecordingObjectEventReceiver
    {
        private readonly Dictionary<int, ReflectionProbe> _recordedReflectionProbes = new();

        protected override void ResetCache()
        {
            _recordedReflectionProbes.Clear();
        }

        public void FixedUpdate()
        {
            if (_recordedReflectionProbes.Count == 0)
                return;

            var nullReflectionProbeInstanceIds = new List<int>();

            foreach (var (reflectionProbeInstanceId, reflectionProbe) in _recordedReflectionProbes)
            {
                if (reflectionProbe == null)
                {
                    nullReflectionProbeInstanceIds.Add(reflectionProbeInstanceId);
                }
            }

            foreach (var nullReflectionProbeInstanceId in nullReflectionProbeInstanceIds)
            {
                RecordDestruction(nullReflectionProbeInstanceId);
                RemoveFromCache(nullReflectionProbeInstanceId);
            }
        }

        private void RemoveFromCache(int reflectionProbeInstanceId)
        {
            _recordedReflectionProbes.Remove(reflectionProbeInstanceId);
        }

        public void OnStartRecordingObject(Object obj)
        {
            if (obj is ReflectionProbe reflectionProbe && !_recordedReflectionProbes.ContainsKey(reflectionProbe.GetInstanceID()))
            {
                _recordedReflectionProbes.Add(reflectionProbe.GetInstanceID(), reflectionProbe);
                RecordCreation(reflectionProbe);
            }
        }

        public void OnStopRecordingObject(int objectInstanceId)
        {
            if (_recordedReflectionProbes.ContainsKey(objectInstanceId))
            {
                RecordDestruction(objectInstanceId);
                RemoveFromCache(objectInstanceId);
            }
        }

        private void RecordCreation(ReflectionProbe reflectionProbe)
        {
            var reflectionProbeCreate = new ReflectionProbeCreate { Id = reflectionProbe.ToIdentifierPayload() };
            var reflectionProbeUpdateEnabled = new ReflectionProbeUpdateEnabled()
            {
                Id = reflectionProbe.ToIdentifierPayload(),
                Enabled = reflectionProbe.enabled
            };
            var reflectionProbeUpdate = new ReflectionProbeUpdate
            {
                Id = reflectionProbe.ToIdentifierPayload(),
                Mode = reflectionProbe.mode.ToPayload(),
                RefreshMode = reflectionProbe.refreshMode.ToPayload(),
                TimeSlicingMode = reflectionProbe.timeSlicingMode.ToPayload(),
                ClearFlags = reflectionProbe.clearFlags.ToPayload(),
                Importance = reflectionProbe.importance,
                Intensity = reflectionProbe.intensity,
                NearClipPlane = reflectionProbe.nearClipPlane,
                FarClipPlane = reflectionProbe.farClipPlane,
                RenderDynamicObjects = reflectionProbe.renderDynamicObjects,
                BoxProjection = reflectionProbe.boxProjection,
                BlendDistance = reflectionProbe.blendDistance,
                Bounds = reflectionProbe.bounds.ToPayload(),
                Resolution = reflectionProbe.resolution,
                Hdr = reflectionProbe.hdr,
                ShadowDistance = reflectionProbe.shadowDistance,
                BackgroundColor = reflectionProbe.backgroundColor.ToPayload(),
                CullingMask = reflectionProbe.cullingMask,
                CustomBakedTextureId = reflectionProbe.customBakedTexture == null ? null : reflectionProbe.customBakedTexture.ToAssetIdentifierPayload(),
                BakedTextureId = reflectionProbe.bakedTexture == null ? null : reflectionProbe.bakedTexture.ToAssetIdentifierPayload()
            };

            recorder.RecordSample(reflectionProbeCreate);
            recorder.RecordSample(reflectionProbeUpdateEnabled);
            recorder.RecordSample(reflectionProbeUpdate);
        }

        private void RecordDestruction(int reflectionProbeInstanceId)
        {
            var reflectionProbeDestroy = new ComponentDestroy
                { Id = new ComponentDestroyIdentifier { Id = reflectionProbeInstanceId.ToString() } };
            recorder.RecordSample(reflectionProbeDestroy);
        }
    }
}