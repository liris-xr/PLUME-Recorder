using System.Collections.Generic;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.UI;
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

        protected override void OnObjectMarkedCreated(CanvasRendererSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);

            var canvasRenderer = objSafeRef.Component;
            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.CullTransparentMesh = canvasRenderer.cullTransparentMesh;
            _createSamples[objSafeRef] = new CanvasRendererCreate { Component = GetComponentIdentifierPayload(objSafeRef) };
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