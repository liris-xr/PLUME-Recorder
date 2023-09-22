using System.Collections.Generic;
using PLUME.Sample.Unity;
using UnityEngine;

namespace PLUME
{
    [DisallowMultipleComponent]
    public class LightRecorderModule : RecorderModule,
        IStartRecordingObjectEventReceiver,
        IStopRecordingObjectEventReceiver
    {
        private readonly Dictionary<int, Light> _recordedLights = new();
        private readonly Dictionary<int, bool> _lastEnabled = new();

        protected override void ResetCache()
        {
            _recordedLights.Clear();
            _lastEnabled.Clear();
        }

        public void OnStartRecordingObject(Object obj)
        {
            // ReSharper disable once LocalVariableHidesMember
            if (obj is Light light && !_recordedLights.ContainsKey(light.GetInstanceID()))
            {
                var lightInstanceId = light.GetInstanceID();
                _recordedLights.Add(lightInstanceId, light);
                _lastEnabled.Add(lightInstanceId, light.enabled);
                RecordCreation(light);
            }
        }

        public void OnStopRecordingObject(int objectInstanceId)
        {
            if (_recordedLights.ContainsKey(objectInstanceId))
            {
                RecordDestruction(objectInstanceId);
                RemoveFromCache(objectInstanceId);
            }
        }

        private void RemoveFromCache(int lightInstanceId)
        {
            _recordedLights.Remove(lightInstanceId);
            _lastEnabled.Remove(lightInstanceId);
        }

        public void FixedUpdate()
        {
            var nullLightsInstanceIds = new List<int>();
            
            foreach (var (lightInstanceId, light) in _recordedLights)
            {
                if (light == null)
                {
                    nullLightsInstanceIds.Add(lightInstanceId);
                    continue;
                }
                
                if (_lastEnabled[lightInstanceId] != light.enabled)
                {
                    _lastEnabled[lightInstanceId] = light.enabled;

                    var lightUpdateEnabled = new LightUpdateEnabled
                    {
                        Id = light.ToIdentifierPayload(),
                        Enabled = light.enabled
                    };

                    recorder.RecordSample(lightUpdateEnabled);
                }
            }

            foreach (var nullLightInstanceId in nullLightsInstanceIds)
            {
                RecordDestruction(nullLightInstanceId);
                RemoveFromCache(nullLightInstanceId);
            }
        }

        private void RecordCreation(Light light)
        {
            var lightIdentifier = light.ToIdentifierPayload();

            var lightCreate = new LightCreate {Id = lightIdentifier};

            var lightUpdateType = new LightUpdateType
            {
                Id = lightIdentifier,
                Type = light.type.ToPayload()
            };

            var lightUpdateIntensity = new LightUpdateIntensity
            {
                Id = lightIdentifier,
                Intensity = light.intensity
            };

            var lightUpdateRange = new LightUpdateRange
            {
                Id = lightIdentifier,
                Range = light.range,
            };

            var lightUpdateColor = new LightUpdateColor
            {
                Id = lightIdentifier,
                Color = light.color.ToPayload(),
                ColorTemperature = light.colorTemperature,
                UseColorTemperature = light.useColorTemperature
            };

            var lightUpdateShape = new LightUpdateShape
            {
                Id = lightIdentifier,
                Shape = light.shape.ToPayload()
            };

            var lightUpdateBounceIntensity = new LightUpdateBounceIntensity
            {
                Id = lightIdentifier,
                BounceIntensity = light.bounceIntensity
            };

            var lightUpdateSpotAngle = new LightUpdateSpotAngle
            {
                Id = lightIdentifier,
                SpotAngle = light.spotAngle,
                InnerSpotAngle = light.innerSpotAngle
            };

            var lightUpdateRenderingLayerMask = new LightUpdateRenderingLayerMask
            {
                Id = lightIdentifier,
                RenderingLayerMask = light.renderingLayerMask
            };

            var lightUpdateCulling = new LightUpdateCulling
            {
                Id = lightIdentifier,
                CullingMask = light.cullingMask,
                BoundingSphereOverride = light.boundingSphereOverride.ToPayload(),
                UseBoundingSphereOverride = light.useBoundingSphereOverride
            };

            var lightUpdateShadows = new LightUpdateShadows
            {
                Id = lightIdentifier,
                Shadows = light.shadows.ToPayload(),
                ShadowStrength = light.shadowStrength,
                ShadowResolution = light.shadowResolution.ToPayload(),
                ShadowMatrixOverride = light.shadowMatrixOverride.ToPayload(),
                UseShadowMatrixOverride = light.useShadowMatrixOverride,
                ShadowBias = light.shadowBias,
                ShadowNormalBias = light.shadowNormalBias,
                ShadowNearPlane = light.shadowNearPlane,
                UseViewFrustumForShadowCasterCull = light.useViewFrustumForShadowCasterCull,
                LayerShadowCullDistances = {light.layerShadowCullDistances},
                ShadowCustomResolution = light.shadowCustomResolution,
                LightShadowCasterMode = light.lightShadowCasterMode.ToPayload()
            };

            var lightUpdateCookie = new LightUpdateCookie
            {
                Id = lightIdentifier,
                CookieId = light.cookie == null ? null : light.cookie.ToAssetIdentifierPayload(),
                CookieSize = light.cookieSize
            };

            var lightUpdateFlare = new LightUpdateFlare
            {
                Id = lightIdentifier,
                FlareId = light.flare == null ? null : light.flare.ToAssetIdentifierPayload()
            };

            var lightUpdateEnabled = new LightUpdateEnabled
            {
                Id = light.ToIdentifierPayload(),
                Enabled = light.enabled
            };
            
            recorder.RecordSample(lightCreate);
            recorder.RecordSample(lightUpdateType);
            recorder.RecordSample(lightUpdateIntensity);
            recorder.RecordSample(lightUpdateRange);
            recorder.RecordSample(lightUpdateColor);
            recorder.RecordSample(lightUpdateShape);
            recorder.RecordSample(lightUpdateBounceIntensity);
            recorder.RecordSample(lightUpdateSpotAngle);
            recorder.RecordSample(lightUpdateRenderingLayerMask);
            recorder.RecordSample(lightUpdateCulling);
            recorder.RecordSample(lightUpdateShadows);
            recorder.RecordSample(lightUpdateCookie);
            recorder.RecordSample(lightUpdateFlare);
            recorder.RecordSample(lightUpdateEnabled);
        }

        private void RecordDestruction(int lightInstanceId)
        {
            var lightDestroy = new ComponentDestroy
                {Id = new ComponentDestroyIdentifier {Id = lightInstanceId.ToString()}};
            recorder.RecordSample(lightDestroy);
        }
    }
}