using System.Collections.Generic;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.UI;
using UnityEngine.Scripting;
using RectTransformSafeRef = PLUME.Core.Object.SafeRef.IComponentSafeRef<UnityEngine.RectTransform>;
using static PLUME.Core.Utils.SampleUtils;
using Vector2 = UnityEngine.Vector2;

namespace PLUME.Base.Module.Unity.UI.RectTransform
{
    /// <summary>
    /// Records the layout of RectTransforms. Their position, rotation, scale and hierarchy are recorded by the
    /// transform module, as for any other transform.
    /// </summary>
    [Preserve]
    public class RectTransformRecorderModule : ComponentRecorderModule<UnityEngine.RectTransform, RectTransformFrameData>
    {
        private readonly Dictionary<RectTransformSafeRef, RectTransformCreate> _createSamples = new();
        private readonly Dictionary<RectTransformSafeRef, RectTransformDestroy> _destroySamples = new();
        private readonly Dictionary<RectTransformSafeRef, RectTransformUpdate> _updateSamples = new();

        // Last recorded layout of each RectTransform, to record only what changed.
        private readonly Dictionary<RectTransformSafeRef, Layout> _layouts = new();

        protected override void OnStartRecordingObject(RectTransformSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnStartRecordingObject(objSafeRef, ctx);
            _layouts[objSafeRef] = new Layout(objSafeRef.Component);
        }

        protected override void OnStopRecordingObject(RectTransformSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnStopRecordingObject(objSafeRef, ctx);
            _layouts.Remove(objSafeRef);
        }

        protected override void OnObjectMarkedCreated(RectTransformSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);

            var layout = new Layout(objSafeRef.Component);
            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.AnchorMin = layout.AnchorMin.ToPayload();
            updateSample.AnchorMax = layout.AnchorMax.ToPayload();
            updateSample.Pivot = layout.Pivot.ToPayload();
            updateSample.SizeDelta = layout.SizeDelta.ToPayload();
            updateSample.AnchoredPosition = layout.AnchoredPosition.ToPayload();
            _createSamples[objSafeRef] = new RectTransformCreate { Component = GetComponentIdentifierPayload(objSafeRef) };
        }

        protected override void OnObjectMarkedDestroyed(RectTransformSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedDestroyed(objSafeRef, ctx);
            _destroySamples[objSafeRef] = new RectTransformDestroy { Component = GetComponentIdentifierPayload(objSafeRef) };
        }

        protected override void OnStopRecording(RecorderContext ctx)
        {
            base.OnStopRecording(ctx);
            _layouts.Clear();
            _createSamples.Clear();
            _destroySamples.Clear();
            _updateSamples.Clear();
        }

        private RectTransformUpdate GetOrCreateUpdateSample(RectTransformSafeRef objSafeRef)
        {
            if (_updateSamples.TryGetValue(objSafeRef, out var sample))
                return sample;
            sample = new RectTransformUpdate { Component = GetComponentIdentifierPayload(objSafeRef) };
            _updateSamples[objSafeRef] = sample;
            return sample;
        }

        private void RecordLayoutChanges()
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < RecordedComponents.Count; i++)
            {
                var objSafeRef = RecordedComponents[i];
                var rectTransform = objSafeRef.Component;

                // Destroyed in this frame, not removed yet.
                if (rectTransform == null)
                    continue;

                var last = _layouts[objSafeRef];
                var layout = new Layout(rectTransform);

                if (layout.Equals(last))
                    continue;

                var updateSample = GetOrCreateUpdateSample(objSafeRef);
                if (layout.AnchorMin != last.AnchorMin)
                    updateSample.AnchorMin = layout.AnchorMin.ToPayload();
                if (layout.AnchorMax != last.AnchorMax)
                    updateSample.AnchorMax = layout.AnchorMax.ToPayload();
                if (layout.Pivot != last.Pivot)
                    updateSample.Pivot = layout.Pivot.ToPayload();
                if (layout.SizeDelta != last.SizeDelta)
                    updateSample.SizeDelta = layout.SizeDelta.ToPayload();
                if (layout.AnchoredPosition != last.AnchoredPosition)
                    updateSample.AnchoredPosition = layout.AnchoredPosition.ToPayload();

                _layouts[objSafeRef] = layout;
            }
        }

        protected override RectTransformFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            RecordLayoutChanges();

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

        private readonly struct Layout
        {
            public readonly Vector2 AnchorMin;
            public readonly Vector2 AnchorMax;
            public readonly Vector2 Pivot;
            public readonly Vector2 SizeDelta;
            public readonly Vector2 AnchoredPosition;

            public Layout(UnityEngine.RectTransform rectTransform)
            {
                AnchorMin = rectTransform.anchorMin;
                AnchorMax = rectTransform.anchorMax;
                Pivot = rectTransform.pivot;
                SizeDelta = rectTransform.sizeDelta;
                AnchoredPosition = rectTransform.anchoredPosition;
            }

            public bool Equals(Layout other) => AnchorMin == other.AnchorMin && AnchorMax == other.AnchorMax &&
                                                Pivot == other.Pivot && SizeDelta == other.SizeDelta &&
                                                AnchoredPosition == other.AnchoredPosition;
        }
    }
}
