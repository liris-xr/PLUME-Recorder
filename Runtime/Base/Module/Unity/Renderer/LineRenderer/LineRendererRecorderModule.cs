using System.Collections.Generic;
using PLUME.Base.Events;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity;
using Unity.Collections;
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

        private NativeList<Vector3> _tmpPositions;

        protected override void OnCreate(RecorderContext ctx)
        {
            base.OnCreate(ctx);
            _tmpPositions = new NativeList<Vector3>(Allocator.Persistent);

            LineRendererEvents.OnPositionsChanged += (r, positions) => OnPositionChanged(r, positions, ctx);
            LineRendererEvents.OnColorChanged += (r, color) => OnColorChanged(r, color, ctx);
            LineRendererEvents.OnWidthCurveChanged += (r, widthCurve) => OnWidthCurveChanged(r, widthCurve, ctx);
        }

        protected override void OnDestroy(RecorderContext ctx)
        {
            base.OnDestroy(ctx);
            _tmpPositions.Dispose();
        }
        
        private void OnWidthCurveChanged(UnityEngine.LineRenderer lineRenderer, AnimationCurve widthCurve, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(lineRenderer);

            if (!IsRecordingObject(objSafeRef))
                return;
            
            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.WidthCurve = widthCurve.ToPayload();
        }
        
        private void OnColorChanged(UnityEngine.LineRenderer lineRenderer, Gradient color, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(lineRenderer);

            if (!IsRecordingObject(objSafeRef))
                return;
            
            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.Color = color.ToPayload();
        }

        private void OnPositionChanged(UnityEngine.LineRenderer lineRenderer, IEnumerable<Vector3> positions, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(lineRenderer);

            if (!IsRecordingObject(objSafeRef))
                return;
            
            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.Positions = new LineRendererUpdate.Types.Positions();
            
            foreach (var position in positions)
            {
                updateSample.Positions.Positions_.Add(position.ToPayload());
            }
        }

        protected override void OnObjectMarkedCreated(LineRendererSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);

            var lineRenderer = objSafeRef.Component;
            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.Loop = lineRenderer.loop;
            updateSample.WidthCurve = lineRenderer.widthCurve.ToPayload();
            updateSample.Positions = new LineRendererUpdate.Types.Positions();
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

            _tmpPositions.ResizeUninitialized(lineRenderer.positionCount);
            
            var nPositions = lineRenderer.GetPositions(_tmpPositions.AsArray());
            for(var i = 0; i < nPositions; ++i)
            {
                updateSample.Positions.Positions_.Add(_tmpPositions[i].ToPayload());
            }
            
            _createSamples[objSafeRef] = new LineRendererCreate { Id = GetComponentIdentifierPayload(objSafeRef) };
        }

        protected override void OnObjectMarkedDestroyed(LineRendererSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedDestroyed(objSafeRef, ctx);
            _destroySamples[objSafeRef] = new LineRendererDestroy { Id = GetComponentIdentifierPayload(objSafeRef) };
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
            sample = new LineRendererUpdate { Id = GetComponentIdentifierPayload(objSafeRef) };
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