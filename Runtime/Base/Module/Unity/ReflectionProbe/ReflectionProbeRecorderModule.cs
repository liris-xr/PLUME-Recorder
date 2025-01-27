using System.Collections.Generic;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity;
using UnityEngine.Scripting;
using static PLUME.Core.Utils.SampleUtils;
using ReflectionProbeSafeRef = PLUME.Core.Object.SafeRef.IComponentSafeRef<UnityEngine.ReflectionProbe>;

namespace PLUME.Base.Module.Unity.ReflectionProbe
{
    [Preserve]
    public class
        ReflectionProbeRecorderModule : ComponentRecorderModule<UnityEngine.ReflectionProbe, ReflectionProbeFrameData>
    {
        private readonly Dictionary<ReflectionProbeSafeRef, ReflectionProbeCreate> _createSamples = new();
        private readonly Dictionary<ReflectionProbeSafeRef, ReflectionProbeDestroy> _destroySamples = new();
        private readonly Dictionary<ReflectionProbeSafeRef, ReflectionProbeUpdate> _updateSamples = new();

        protected override void OnCreate(RecorderContext ctx)
        {
            base.OnCreate(ctx);
            // TODO: add hooks
        }

        protected override void OnObjectMarkedCreated(ReflectionProbeSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);

            var customBakedTexture =
                ctx.SafeRefProvider.GetOrCreateAssetSafeRef(objSafeRef.Component.customBakedTexture);
            var bakedTexture = ctx.SafeRefProvider.GetOrCreateAssetSafeRef(objSafeRef.Component.bakedTexture);

            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.Enabled = objSafeRef.Component.enabled;
            updateSample.Mode = objSafeRef.Component.mode.ToPayload();
            updateSample.RefreshMode = objSafeRef.Component.refreshMode.ToPayload();
            updateSample.TimeSlicingMode = objSafeRef.Component.timeSlicingMode.ToPayload();
            updateSample.ClearFlags = objSafeRef.Component.clearFlags.ToPayload();
            updateSample.Importance = objSafeRef.Component.importance;
            updateSample.Intensity = objSafeRef.Component.intensity;
            updateSample.NearClipPlane = objSafeRef.Component.nearClipPlane;
            updateSample.FarClipPlane = objSafeRef.Component.farClipPlane;
            updateSample.RenderDynamicObjects = objSafeRef.Component.renderDynamicObjects;
            updateSample.BoxProjection = objSafeRef.Component.boxProjection;
            updateSample.BlendDistance = objSafeRef.Component.blendDistance;
            updateSample.Bounds = objSafeRef.Component.bounds.ToPayload();
            updateSample.Resolution = objSafeRef.Component.resolution;
            updateSample.Hdr = objSafeRef.Component.hdr;
            updateSample.ShadowDistance = objSafeRef.Component.shadowDistance;
            updateSample.BackgroundColor = objSafeRef.Component.backgroundColor.ToPayload();
            updateSample.CullingMask = objSafeRef.Component.cullingMask;
            updateSample.CustomBakedTexture = GetAssetIdentifierPayload(customBakedTexture);
            updateSample.BakedTexture = GetAssetIdentifierPayload(bakedTexture);
            _createSamples[objSafeRef] = new ReflectionProbeCreate
                { Component = GetComponentIdentifierPayload(objSafeRef) };
        }

        protected override void OnObjectMarkedDestroyed(ReflectionProbeSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedDestroyed(objSafeRef, ctx);
            _destroySamples[objSafeRef] = new ReflectionProbeDestroy
                { Component = GetComponentIdentifierPayload(objSafeRef) };
        }

        private ReflectionProbeUpdate GetOrCreateUpdateSample(ReflectionProbeSafeRef objSafeRef)
        {
            if (_updateSamples.TryGetValue(objSafeRef, out var sample))
                return sample;
            sample = new ReflectionProbeUpdate { Component = GetComponentIdentifierPayload(objSafeRef) };
            _updateSamples[objSafeRef] = sample;
            return sample;
        }

        protected override ReflectionProbeFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            var frameData = ReflectionProbeFrameData.Pool.Get();
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