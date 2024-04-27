using System.Collections.Generic;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity;
using UnityEngine.Scripting;
using LightSafeRef = PLUME.Core.Object.SafeRef.IComponentSafeRef<UnityEngine.Light>;
using static PLUME.Core.Utils.SampleUtils;

namespace PLUME.Base.Module.Unity.Light
{
    [Preserve]
    public class LightRecorderModule : ComponentRecorderModule<UnityEngine.Light, LightFrameData>
    {
        private readonly Dictionary<LightSafeRef, LightCreate> _createSamples = new();
        private readonly Dictionary<LightSafeRef, LightDestroy> _destroySamples = new();
        private readonly Dictionary<LightSafeRef, LightUpdate> _updateSamples = new();

        protected override void OnObjectMarkedCreated(LightSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);

            var light = objSafeRef.Component;
            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.Enabled = light.enabled;
            updateSample.Type = light.type.ToPayload();
            updateSample.Shape = light.shape.ToPayload();
            updateSample.Intensity = light.intensity;
            updateSample.BounceIntensity = light.bounceIntensity;
            updateSample.Range = light.range;
            updateSample.Color = light.color.ToPayload();
            updateSample.ColorTemperature = light.colorTemperature;
            updateSample.UseColorTemperature = light.useColorTemperature;
            updateSample.SpotAngle = light.spotAngle;
            updateSample.InnerSpotAngle = light.innerSpotAngle;
            updateSample.Shadows = light.shadows.ToPayload();
            updateSample.ShadowStrength = light.shadowStrength;
            updateSample.ShadowResolution = light.shadowResolution.ToPayload();
            updateSample.ShadowMatrixOverride = light.shadowMatrixOverride.ToPayload();
            updateSample.UseShadowMatrixOverride = light.useShadowMatrixOverride;
            updateSample.ShadowBias = light.shadowBias;
            updateSample.ShadowNormalBias = light.shadowNormalBias;
            updateSample.ShadowNearPlane = light.shadowNearPlane;
            updateSample.UseViewFrustumForShadowCasterCull = light.useViewFrustumForShadowCasterCull;
            updateSample.LayerShadowCullDistances = new LayerShadowCullDistances();
            updateSample.LayerShadowCullDistances.Distances.AddRange(light.layerShadowCullDistances);
            updateSample.ShadowCustomResolution = light.shadowCustomResolution;
            updateSample.LightShadowCasterMode = light.lightShadowCasterMode.ToPayload();
            updateSample.RenderingLayerMask = light.renderingLayerMask;
            updateSample.CullingMask = light.cullingMask;
            updateSample.BoundingSphereOverride = light.boundingSphereOverride.ToPayload();
            updateSample.UseBoundingSphereOverride = light.useBoundingSphereOverride;
            updateSample.CookieId = GetAssetIdentifierPayload(light.cookie);
            updateSample.CookieSize = light.cookieSize;
            updateSample.FlareId = GetAssetIdentifierPayload(light.flare);
            _createSamples[objSafeRef] = new LightCreate { Id = GetComponentIdentifierPayload(objSafeRef) };
        }

        protected override void OnObjectMarkedDestroyed(LightSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedDestroyed(objSafeRef, ctx);
            _destroySamples[objSafeRef] = new LightDestroy { Id = GetComponentIdentifierPayload(objSafeRef) };
        }

        private LightUpdate GetOrCreateUpdateSample(LightSafeRef objSafeRef)
        {
            if (_updateSamples.TryGetValue(objSafeRef, out var sample))
                return sample;
            sample = new LightUpdate { Id = GetComponentIdentifierPayload(objSafeRef) };
            _updateSamples[objSafeRef] = sample;
            return sample;
        }

        protected override LightFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            var frameData = LightFrameData.Pool.Get();
            frameData.AddCreateSamples(_createSamples.Values);
            frameData.AddDestroySamples(_destroySamples.Values);
            frameData.AddUpdateSamples(_updateSamples.Values);
            return frameData;
        }

        protected override void AfterCollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            base.AfterCollectFrameData(frameInfo, ctx);
            _createSamples.Clear();
            _destroySamples.Clear();
            _updateSamples.Clear();
        }
    }
}