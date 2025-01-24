using System.Collections.Generic;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity;
using PLUME.Sample.Unity.UI;
using UnityEngine.Scripting;
using RectTransformSafeRef = PLUME.Core.Object.SafeRef.IComponentSafeRef<UnityEngine.RectTransform>;
using static PLUME.Core.Utils.SampleUtils;

namespace PLUME.Base.Module.Unity.UI.RectTransform
{
    [Preserve]
    public class RectTransformRecorderModule : ComponentRecorderModule<UnityEngine.RectTransform, RectTransformFrameData>
    {
        private readonly Dictionary<RectTransformSafeRef, RectTransformCreate> _createSamples = new();
        private readonly Dictionary<RectTransformSafeRef, RectTransformDestroy> _destroySamples = new();
        private readonly Dictionary<RectTransformSafeRef, RectTransformUpdate> _updateSamples = new();
        private readonly Dictionary<RectTransformSafeRef, TransformUpdate> _transformUpdateSamples = new();
        
        protected override void OnObjectMarkedCreated(RectTransformSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);
            
            var rectTransform = objSafeRef.Component;
            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.LocalPosition = rectTransform.localPosition.ToPayload();
            updateSample.LocalRotation = rectTransform.localRotation.ToPayload();
            updateSample.LocalScale = rectTransform.localScale.ToPayload();
            updateSample.ParentTransform = GetComponentIdentifierPayload(rectTransform.parent);
            updateSample.SiblingIdx = rectTransform.GetSiblingIndex();
            updateSample.AnchorMin = rectTransform.anchorMin.ToPayload();
            updateSample.AnchorMax = rectTransform.anchorMax.ToPayload();
            updateSample.Pivot = rectTransform.pivot.ToPayload();
            updateSample.SizeDelta = rectTransform.sizeDelta.ToPayload();
            updateSample.AnchoredPosition = rectTransform.anchoredPosition.ToPayload();
            _createSamples[objSafeRef] = new RectTransformCreate { Component = GetComponentIdentifierPayload(objSafeRef) };
        }

        protected override void OnObjectMarkedDestroyed(RectTransformSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedDestroyed(objSafeRef, ctx);
            _destroySamples[objSafeRef] = new RectTransformDestroy { Component = GetComponentIdentifierPayload(objSafeRef) };
        }
        
        private RectTransformUpdate GetOrCreateUpdateSample(RectTransformSafeRef objSafeRef)
        {
            if (_updateSamples.TryGetValue(objSafeRef, out var sample))
                return sample;
            sample = new RectTransformUpdate { Component = GetComponentIdentifierPayload(objSafeRef) };
            _updateSamples[objSafeRef] = sample;
            return sample;
        }

        protected override RectTransformFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            var frameData = RectTransformFrameData.Pool.Get();
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