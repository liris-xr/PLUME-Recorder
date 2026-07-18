#if HDRP_ENABLED
using System.Collections.Generic;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.HDRP;
using UnityEngine.Scripting;
using HDAdditionalLightDataSafeRef =
    PLUME.Core.Object.SafeRef.IComponentSafeRef<UnityEngine.Rendering.HighDefinition.HDAdditionalLightData>;
using static PLUME.Core.Utils.SampleUtils;

namespace PLUME.Base.Module.Unity.HDRP
{
    [Preserve]
    public class HDAdditionalLightDataRecorderModule : ComponentRecorderModule<
        UnityEngine.Rendering.HighDefinition.HDAdditionalLightData, HDAdditionalLightDataFrameData>
    {
        private readonly Dictionary<HDAdditionalLightDataSafeRef, HDAdditionalLightDataCreate> _createSamples = new();
        private readonly Dictionary<HDAdditionalLightDataSafeRef, HDAdditionalLightDataDestroy> _destroySamples = new();
        private readonly Dictionary<HDAdditionalLightDataSafeRef, HDAdditionalLightDataUpdate> _updateSamples = new();

        protected override void OnObjectMarkedCreated(HDAdditionalLightDataSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);

            var lightData = objSafeRef.Component;
            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.InteractsWithSky = lightData.interactsWithSky;
            updateSample.AffectDiffuse = lightData.affectDiffuse;
            updateSample.AffectSpecular = lightData.affectSpecular;
            updateSample.AffectsVolumetric = lightData.affectsVolumetric;
            updateSample.LightDimmer = lightData.lightDimmer;
            updateSample.VolumetricDimmer = lightData.volumetricDimmer;
            updateSample.ShadowDimmer = lightData.shadowDimmer;
            updateSample.VolumetricShadowDimmer = lightData.volumetricShadowDimmer;
            updateSample.VolumetricFadeDistance = lightData.volumetricFadeDistance;
            updateSample.ShapeRadius = lightData.shapeRadius;
            updateSample.ApplyRangeAttenuation = lightData.applyRangeAttenuation;
            updateSample.SurfaceTint = lightData.surfaceTint.ToPayload();
            updateSample.ShadowTint = lightData.shadowTint.ToPayload();
            _createSamples[objSafeRef] = new HDAdditionalLightDataCreate
            {
                Component = GetComponentIdentifierPayload(objSafeRef)
            };
        }

        protected override void OnObjectMarkedDestroyed(HDAdditionalLightDataSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedDestroyed(objSafeRef, ctx);
            _destroySamples[objSafeRef] =
                new HDAdditionalLightDataDestroy { Component = GetComponentIdentifierPayload(objSafeRef) };
        }

        private HDAdditionalLightDataUpdate GetOrCreateUpdateSample(HDAdditionalLightDataSafeRef objSafeRef)
        {
            if (_updateSamples.TryGetValue(objSafeRef, out var sample))
                return sample;
            sample = new HDAdditionalLightDataUpdate { Component = GetComponentIdentifierPayload(objSafeRef) };
            _updateSamples[objSafeRef] = sample;
            return sample;
        }

        protected override HDAdditionalLightDataFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            var frameData = HDAdditionalLightDataFrameData.Pool.Get();
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
#endif
