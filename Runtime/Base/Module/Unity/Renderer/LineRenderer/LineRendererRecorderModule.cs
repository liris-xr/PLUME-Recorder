using System.Collections.Generic;
using PLUME.Base.Hooks;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity;
using UnityEngine;
using UnityEngine.Scripting;
using LineRendererSafeRef = PLUME.Core.Object.SafeRef.IComponentSafeRef<UnityEngine.LineRenderer>;
using static PLUME.Core.Utils.SampleUtils;

namespace PLUME.Base.Module.Unity.Renderer.LineRenderer
{
    [Preserve]
    public class LineRendererRecorderModule : RendererRecorderModule<UnityEngine.LineRenderer, LineRendererFrameData>
    {
        private readonly Dictionary<LineRendererSafeRef, LineRendererCreate> _createSamples = new();
        private readonly Dictionary<LineRendererSafeRef, LineRendererDestroy> _destroySamples = new();
        private readonly Dictionary<LineRendererSafeRef, LineRendererUpdate> _updateSamples = new();

        private struct LineState
        {
            public Color StartColor;
            public Color EndColor;
            public float StartWidth;
            public float EndWidth;
            public float WidthMultiplier;
            public bool Loop;
            public bool UseWorldSpace;
        }

        // Last recorded state per line, compared every frame. Only allocation-free getters are polled; the gradient
        // and width curve (whose getters allocate) are read only when their endpoints change. Changes confined to
        // inner gradient or curve keys are still caught by the setter hooks below when they run.
        private readonly Dictionary<LineRendererSafeRef, LineState> _states = new();

        protected override void OnCreate(RecorderContext ctx)
        {
            base.OnCreate(ctx);

            // Positions are recorded by LineRendererPositionsRecorderModule, which polls them allocation-free.
            LineRendererHooks.OnColorChanged += (r, color) => OnColorChanged(r, color, ctx);
            LineRendererHooks.OnWidthCurveChanged += (r, widthCurve) => OnWidthCurveChanged(r, widthCurve, ctx);
            LineRendererHooks.OnWidthMultiplierChanged += (r, widthMultiplier) => OnWidthMultiplierChanged(r, widthMultiplier, ctx);
        }

        protected override void OnStopRecordingObject(LineRendererSafeRef objSafeRef, RecorderContext ctx)
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
                PollLine(recorded[i]);
        }

        private static LineState ReadLineState(UnityEngine.LineRenderer lineRenderer)
        {
            return new LineState
            {
                StartColor = lineRenderer.startColor,
                EndColor = lineRenderer.endColor,
                StartWidth = lineRenderer.startWidth,
                EndWidth = lineRenderer.endWidth,
                WidthMultiplier = lineRenderer.widthMultiplier,
                Loop = lineRenderer.loop,
                UseWorldSpace = lineRenderer.useWorldSpace
            };
        }

        private void PollLine(LineRendererSafeRef objSafeRef)
        {
            var lineRenderer = objSafeRef.Component;

            if (lineRenderer == null)
                return;

            var current = ReadLineState(lineRenderer);

            // No baseline yet (recording started without a creation mark): store it, the creation sample or the
            // hooks hold the full state.
            if (!_states.TryGetValue(objSafeRef, out var previous))
            {
                _states[objSafeRef] = current;
                return;
            }

            // XR interactor lines switch between valid/invalid gradients every frame from package code.
            if (previous.StartColor != current.StartColor || previous.EndColor != current.EndColor)
                GetOrCreateUpdateSample(objSafeRef).Color = lineRenderer.colorGradient.ToPayload();
            if (!Mathf.Approximately(previous.StartWidth, current.StartWidth) ||
                !Mathf.Approximately(previous.EndWidth, current.EndWidth))
                GetOrCreateUpdateSample(objSafeRef).WidthCurve = lineRenderer.widthCurve.ToPayload();
            if (!Mathf.Approximately(previous.WidthMultiplier, current.WidthMultiplier))
                GetOrCreateUpdateSample(objSafeRef).WidthMultiplier = current.WidthMultiplier;
            if (previous.Loop != current.Loop)
                GetOrCreateUpdateSample(objSafeRef).Loop = current.Loop;
            if (previous.UseWorldSpace != current.UseWorldSpace)
                GetOrCreateUpdateSample(objSafeRef).UseWorldSpace = current.UseWorldSpace;

            _states[objSafeRef] = current;
        }
        
        private void OnWidthCurveChanged(UnityEngine.LineRenderer lineRenderer, AnimationCurve widthCurve, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.SafeRefProvider.GetOrCreateComponentSafeRef(lineRenderer);

            if (!IsRecordingObject(objSafeRef))
                return;
            
            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.WidthCurve = widthCurve.ToPayload();
        }
        
        private void OnWidthMultiplierChanged(UnityEngine.LineRenderer lineRenderer, float widthMultiplier, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.SafeRefProvider.GetOrCreateComponentSafeRef(lineRenderer);

            if (!IsRecordingObject(objSafeRef))
                return;
            
            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.WidthMultiplier = widthMultiplier;
        }
        
        private void OnColorChanged(UnityEngine.LineRenderer lineRenderer, Gradient color, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.SafeRefProvider.GetOrCreateComponentSafeRef(lineRenderer);

            if (!IsRecordingObject(objSafeRef))
                return;
            
            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.Color = color.ToPayload();
        }


        protected override void OnObjectMarkedCreated(LineRendererSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);

            var lineRenderer = objSafeRef.Component;
            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.Loop = lineRenderer.loop;
            updateSample.WidthCurve = lineRenderer.widthCurve.ToPayload();
            updateSample.Color = lineRenderer.colorGradient.ToPayload();
            updateSample.CornerVertices = lineRenderer.numCornerVertices;
            updateSample.EndCapVertices = lineRenderer.numCapVertices;
            updateSample.Alignment = lineRenderer.alignment.ToPayload();
            updateSample.TextureMode = lineRenderer.textureMode.ToPayload();
            updateSample.TextureScale = lineRenderer.textureScale.ToPayload();
            updateSample.ShadowBias = lineRenderer.shadowBias;
            updateSample.GenerateLightingData = lineRenderer.generateLightingData;
            updateSample.UseWorldSpace = lineRenderer.useWorldSpace;
            updateSample.MaskInteraction = lineRenderer.maskInteraction.ToPayload();
            updateSample.WidthMultiplier = lineRenderer.widthMultiplier;
            _states[objSafeRef] = ReadLineState(lineRenderer);

            _createSamples[objSafeRef] = new LineRendererCreate { Component = GetComponentIdentifierPayload(objSafeRef) };
        }

        protected override void OnObjectMarkedDestroyed(LineRendererSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedDestroyed(objSafeRef, ctx);
            _destroySamples[objSafeRef] = new LineRendererDestroy { Component = GetComponentIdentifierPayload(objSafeRef) };
        }

        protected override LineRendererFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            var frameData = LineRendererFrameData.Pool.Get();
            frameData.AddCreateSamples(_createSamples.Values);
            frameData.AddDestroySamples(_destroySamples.Values);
            frameData.AddUpdateSamples(_updateSamples.Values);
            frameData.AddUpdateSamples(GetRendererUpdateSamples());
            return frameData;
        }
        
        private LineRendererUpdate GetOrCreateUpdateSample(LineRendererSafeRef objSafeRef)
        {
            if (_updateSamples.TryGetValue(objSafeRef, out var sample))
                return sample;
            sample = new LineRendererUpdate { Component = GetComponentIdentifierPayload(objSafeRef) };
            _updateSamples[objSafeRef] = sample;
            return sample;
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