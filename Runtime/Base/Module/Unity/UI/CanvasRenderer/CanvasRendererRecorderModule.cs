using System.Collections.Generic;
using PLUME.Base.Module.Unity.UI.Common;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.UI;
using UnityEngine;
using UnityEngine.Scripting;
using CanvasRendererSafeRef = PLUME.Core.Object.SafeRef.IComponentSafeRef<UnityEngine.CanvasRenderer>;
using static PLUME.Core.Utils.SampleUtils;

namespace PLUME.Base.Module.Unity.UI.CanvasRenderer
{
    [Preserve]
    public class CanvasRendererRecorderModule : ComponentRecorderModule<UnityEngine.CanvasRenderer, CanvasRendererFrameData>
    {
        private readonly Dictionary<CanvasRendererSafeRef, CanvasRendererCreate> _createSamples = new();
        private readonly Dictionary<CanvasRendererSafeRef, CanvasRendererDestroy> _destroySamples = new();
        private readonly Dictionary<CanvasRendererSafeRef, CanvasRendererUpdate> _updateSamples = new();

        private struct CanvasRendererState
        {
            public bool CullTransparentMesh;
            public Color Color;
        }

        // Last recorded state per canvas renderer, compared every frame.
        private readonly Dictionary<CanvasRendererSafeRef, CanvasRendererState> _states = new();

        protected override void OnObjectMarkedCreated(CanvasRendererSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);

            _createSamples[objSafeRef] = new CanvasRendererCreate { Component = GetComponentIdentifierPayload(objSafeRef) };
            _states.Remove(objSafeRef);
            PollCanvasRenderer(objSafeRef);
        }

        protected override void OnStopRecordingObject(CanvasRendererSafeRef objSafeRef, RecorderContext ctx)
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
                PollCanvasRenderer(recorded[i]);
        }

        private void PollCanvasRenderer(CanvasRendererSafeRef objSafeRef)
        {
            var canvasRenderer = objSafeRef.Component;

            if (canvasRenderer == null)
                return;

            var current = new CanvasRendererState
            {
                CullTransparentMesh = canvasRenderer.cullTransparentMesh,
                Color = canvasRenderer.GetColor()
            };

            var has = _states.TryGetValue(objSafeRef, out var previous);

            if (UIDiff.Changed(has, previous.CullTransparentMesh, current.CullTransparentMesh))
                GetOrCreateUpdateSample(objSafeRef).CullTransparentMesh = current.CullTransparentMesh;
            // Selectable color tint transitions and CrossFadeColor/CrossFadeAlpha tweens only write this color.
            if (UIDiff.Changed(has, previous.Color, current.Color))
                GetOrCreateUpdateSample(objSafeRef).Color = current.Color.ToPayload();

            _states[objSafeRef] = current;
        }

        protected override void OnObjectMarkedDestroyed(CanvasRendererSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedDestroyed(objSafeRef, ctx);
            _destroySamples[objSafeRef] = new CanvasRendererDestroy { Component = GetComponentIdentifierPayload(objSafeRef) };
        }

        private CanvasRendererUpdate GetOrCreateUpdateSample(CanvasRendererSafeRef objSafeRef)
        {
            if (_updateSamples.TryGetValue(objSafeRef, out var sample))
                return sample;
            sample = new CanvasRendererUpdate { Component = GetComponentIdentifierPayload(objSafeRef) };
            _updateSamples[objSafeRef] = sample;
            return sample;
        }

        protected override CanvasRendererFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            var frameData = CanvasRendererFrameData.Pool.Get();
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