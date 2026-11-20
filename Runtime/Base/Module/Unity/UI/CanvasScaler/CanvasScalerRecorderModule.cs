using System.Collections.Generic;
using PLUME.Base.Module.Unity.UI.Common;
using PLUME.Base.Module.Unity.UI.Canvas;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.UI;
using UnityEngine;
using UnityEngine.Scripting;
using CanvasSafeRef = PLUME.Core.Object.SafeRef.IComponentSafeRef<UnityEngine.Canvas>;
using static PLUME.Core.Utils.SampleUtils;

namespace PLUME.Base.Module.Unity.UI.CanvasScaler
{
    [Preserve]
    public class CanvasRecorderModule : ComponentRecorderModule<UnityEngine.Canvas, CanvasFrameData>
    {
        private readonly Dictionary<CanvasSafeRef, CanvasCreate> _createSamples = new();
        private readonly Dictionary<CanvasSafeRef, CanvasDestroy> _destroySamples = new();
        private readonly Dictionary<CanvasSafeRef, CanvasUpdate> _updateSamples = new();

        private struct CanvasState
        {
            public bool Enabled;
            public UnityEngine.RenderMode RenderMode;
            public float PlaneDistance;
            public bool OverrideSorting;
            public int SortingOrder;
            public UnityEngine.Camera WorldCamera;
        }

        // Last recorded runtime-mutable state per canvas, compared every frame. Enabling and disabling a canvas is a
        // common way to show and hide a whole UI.
        private readonly Dictionary<CanvasSafeRef, CanvasState> _states = new();

        protected override void OnObjectMarkedCreated(CanvasSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);
            
            var canvas = objSafeRef.Component;
            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.RenderMode = canvas.renderMode.ToPayload();
            updateSample.ScaleFactor = canvas.scaleFactor;
            updateSample.ReferencePixelsPerUnit = canvas.referencePixelsPerUnit;
            updateSample.OverridePixelPerfect = canvas.overridePixelPerfect;
            updateSample.VertexColorAlwaysGammaSpace = canvas.vertexColorAlwaysGammaSpace;
            updateSample.PixelPerfect = canvas.pixelPerfect;
            updateSample.PlaneDistance = canvas.planeDistance;
            updateSample.OverrideSorting = canvas.overrideSorting;
            updateSample.SortingOrder = canvas.sortingOrder;
            updateSample.TargetDisplay = canvas.targetDisplay;
            updateSample.SortingLayerId = canvas.sortingLayerID;
            updateSample.AdditionalShaderChannels = (uint) canvas.additionalShaderChannels;
            updateSample.SortingLayerName = canvas.sortingLayerName;
            updateSample.UpdateRectTransformForStandalone = canvas.updateRectTransformForStandalone.ToPayload();
            updateSample.WorldCamera = GetComponentIdentifierPayload(canvas.worldCamera);
            updateSample.NormalizedSortingGridSize = canvas.normalizedSortingGridSize;
            updateSample.Enabled = canvas.enabled;
            _createSamples[objSafeRef] = new CanvasCreate { Component = GetComponentIdentifierPayload(objSafeRef) };
            _states[objSafeRef] = ReadCanvasState(canvas);
        }

        protected override void OnStopRecordingObject(CanvasSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnStopRecordingObject(objSafeRef, ctx);
            _states.Remove(objSafeRef);
        }

        protected override void OnStopRecording(RecorderContext ctx)
        {
            base.OnStopRecording(ctx);
            _states.Clear();
        }

        protected override void BeforeCollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            base.BeforeCollectFrameData(frameInfo, ctx);

            var recorded = RecordedComponents;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < recorded.Count; i++)
                PollCanvas(recorded[i]);
        }

        private static CanvasState ReadCanvasState(UnityEngine.Canvas canvas)
        {
            return new CanvasState
            {
                Enabled = canvas.enabled,
                RenderMode = canvas.renderMode,
                PlaneDistance = canvas.planeDistance,
                OverrideSorting = canvas.overrideSorting,
                SortingOrder = canvas.sortingOrder,
                WorldCamera = canvas.worldCamera
            };
        }

        private void PollCanvas(CanvasSafeRef objSafeRef)
        {
            var canvas = objSafeRef.Component;

            if (canvas == null)
                return;

            var current = ReadCanvasState(canvas);
            var has = _states.TryGetValue(objSafeRef, out var previous);

            if (UIDiff.Changed(has, previous.Enabled, current.Enabled))
                GetOrCreateUpdateSample(objSafeRef).Enabled = current.Enabled;
            if (UIDiff.Changed(has, (int)previous.RenderMode, (int)current.RenderMode))
                GetOrCreateUpdateSample(objSafeRef).RenderMode = current.RenderMode.ToPayload();
            if (UIDiff.Changed(has, previous.PlaneDistance, current.PlaneDistance))
                GetOrCreateUpdateSample(objSafeRef).PlaneDistance = current.PlaneDistance;
            if (UIDiff.Changed(has, previous.OverrideSorting, current.OverrideSorting))
                GetOrCreateUpdateSample(objSafeRef).OverrideSorting = current.OverrideSorting;
            if (UIDiff.Changed(has, previous.SortingOrder, current.SortingOrder))
                GetOrCreateUpdateSample(objSafeRef).SortingOrder = current.SortingOrder;
            if (UIDiff.Changed(has, previous.WorldCamera, current.WorldCamera))
                GetOrCreateUpdateSample(objSafeRef).WorldCamera = GetComponentIdentifierPayload(current.WorldCamera);

            _states[objSafeRef] = current;
        }

        protected override void OnObjectMarkedDestroyed(CanvasSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedDestroyed(objSafeRef, ctx);
            _destroySamples[objSafeRef] = new CanvasDestroy { Component = GetComponentIdentifierPayload(objSafeRef) };
        }

        private CanvasUpdate GetOrCreateUpdateSample(CanvasSafeRef objSafeRef)
        {
            if (_updateSamples.TryGetValue(objSafeRef, out var sample))
                return sample;
            sample = new CanvasUpdate { Component = GetComponentIdentifierPayload(objSafeRef) };
            _updateSamples[objSafeRef] = sample;
            return sample;
        }

        protected override CanvasFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            var frameData = CanvasFrameData.Pool.Get();
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