using System.Collections.Generic;
using PLUME.Base.Module.Unity.UI.Common;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity;
using PLUME.Sample.Unity.UI;
using UnityEngine;
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

        private struct RectState
        {
            public Vector2 AnchorMin;
            public Vector2 AnchorMax;
            public Vector2 Pivot;
            public Vector2 SizeDelta;
            public Vector2 AnchoredPosition;
        }

        // Last recorded rect per RectTransform, compared every frame. Layout groups, content size fitters, aspect
        // ratio fitters and scripts resize and move RectTransforms after creation, from inside Unity's layout
        // rebuild where setter hooks never run.
        private readonly Dictionary<RectTransformSafeRef, RectState> _rectStates = new();
        
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
            _rectStates[objSafeRef] = ReadRectState(rectTransform);
        }

        protected override void OnStopRecordingObject(RectTransformSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnStopRecordingObject(objSafeRef, ctx);
            _rectStates.Remove(objSafeRef);
        }

        protected override void OnStopRecording(RecorderContext ctx)
        {
            base.OnStopRecording(ctx);
            _rectStates.Clear();
        }

        protected override void BeforeCollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            base.BeforeCollectFrameData(frameInfo, ctx);

            var recorded = RecordedComponents;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < recorded.Count; i++)
                PollRect(recorded[i]);
        }

        private static RectState ReadRectState(UnityEngine.RectTransform rectTransform)
        {
            return new RectState
            {
                AnchorMin = rectTransform.anchorMin,
                AnchorMax = rectTransform.anchorMax,
                Pivot = rectTransform.pivot,
                SizeDelta = rectTransform.sizeDelta,
                AnchoredPosition = rectTransform.anchoredPosition
            };
        }

        private void PollRect(RectTransformSafeRef objSafeRef)
        {
            var rectTransform = objSafeRef.Component;

            if (rectTransform == null)
                return;

            var current = ReadRectState(rectTransform);
            var has = _rectStates.TryGetValue(objSafeRef, out var previous);

            if (UIDiff.Changed(has, previous.AnchorMin, current.AnchorMin))
                GetOrCreateUpdateSample(objSafeRef).AnchorMin = current.AnchorMin.ToPayload();
            if (UIDiff.Changed(has, previous.AnchorMax, current.AnchorMax))
                GetOrCreateUpdateSample(objSafeRef).AnchorMax = current.AnchorMax.ToPayload();
            if (UIDiff.Changed(has, previous.Pivot, current.Pivot))
                GetOrCreateUpdateSample(objSafeRef).Pivot = current.Pivot.ToPayload();
            if (UIDiff.Changed(has, previous.SizeDelta, current.SizeDelta))
                GetOrCreateUpdateSample(objSafeRef).SizeDelta = current.SizeDelta.ToPayload();
            if (UIDiff.Changed(has, previous.AnchoredPosition, current.AnchoredPosition))
                GetOrCreateUpdateSample(objSafeRef).AnchoredPosition = current.AnchoredPosition.ToPayload();

            _rectStates[objSafeRef] = current;
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