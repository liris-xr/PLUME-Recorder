using System.Collections.Generic;
using PLUME.Base.Module.Unity.UI.Canvas;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Core.Utils;
using PLUME.Sample.Unity.UI;
using UnityEngine.Scripting;
using CanvasSafeRef = PLUME.Core.Object.SafeRef.IComponentSafeRef<UnityEngine.Canvas>;

namespace PLUME.Base.Module.Unity.Canvas
{
    [Preserve]
    public class CanvasRecorderModule : ComponentRecorderModule<UnityEngine.Canvas, CanvasFrameData>
    {
        private readonly Dictionary<CanvasSafeRef, CanvasCreate> _createSamples = new();
        private readonly Dictionary<CanvasSafeRef, CanvasDestroy> _destroySamples = new();
        private readonly Dictionary<CanvasSafeRef, CanvasUpdate> _updateSamples = new();

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
            updateSample.AdditionalShaderChannels = canvas.additionalShaderChannels.ToPayload();
            updateSample.SortingLayerName = canvas.sortingLayerName;
            updateSample.UpdateRectTransformForStandalone = canvas.updateRectTransformForStandalone.ToPayload();
            updateSample.WorldCamera = canvas.worldCamera.ToIdentifierPayload();
            updateSample.NormalizedSortingGridSize = canvas.normalizedSortingGridSize;
            _createSamples[objSafeRef] = new CanvasCreate { Id = objSafeRef.ToIdentifierPayload() };
        }

        protected override void OnObjectMarkedDestroyed(CanvasSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedDestroyed(objSafeRef, ctx);
            _destroySamples[objSafeRef] = new CanvasDestroy { Id = objSafeRef.ToIdentifierPayload() };
        }

        private CanvasUpdate GetOrCreateUpdateSample(CanvasSafeRef objSafeRef)
        {
            if (_updateSamples.TryGetValue(objSafeRef, out var sample))
                return sample;
            sample = new CanvasUpdate { Id = objSafeRef.ToIdentifierPayload() };
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

        protected override void OnAfterCollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            base.OnAfterCollectFrameData(frameInfo, ctx);
            _createSamples.Clear();
            _destroySamples.Clear();
            _updateSamples.Clear();
        }
    }
}