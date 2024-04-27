using System.Collections.Generic;
using PLUME.Base.Module.Unity.UI.CanvasScaler;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.UI;
using UnityEngine.Scripting;
using CanvasScalerSafeRef = PLUME.Core.Object.SafeRef.IComponentSafeRef<UnityEngine.UI.CanvasScaler>;
using static PLUME.Core.Utils.SampleUtils;

namespace PLUME.Base.Module.Unity.UI.Canvas
{
    [Preserve]
    public class
        CanvasScalerRecorderModule : ComponentRecorderModule<UnityEngine.UI.CanvasScaler, CanvasScalerFrameData>
    {
        private readonly Dictionary<CanvasScalerSafeRef, CanvasScalerCreate> _createSamples = new();
        private readonly Dictionary<CanvasScalerSafeRef, CanvasScalerDestroy> _destroySamples = new();
        private readonly Dictionary<CanvasScalerSafeRef, CanvasScalerUpdate> _updateSamples = new();

        protected override void OnObjectMarkedCreated(CanvasScalerSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);

            var canvasScaler = objSafeRef.Component;
            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.UiScaleMode = canvasScaler.uiScaleMode.ToPayload();
            updateSample.ReferencePixelsPerUnit = canvasScaler.referencePixelsPerUnit;
            updateSample.ScaleFactor = canvasScaler.scaleFactor;
            updateSample.ReferenceResolution = canvasScaler.referenceResolution.ToPayload();
            updateSample.ScreenMatchMode = canvasScaler.screenMatchMode.ToPayload();
            updateSample.MatchWidthOrHeight = canvasScaler.matchWidthOrHeight;
            updateSample.PhysicalUnit = canvasScaler.physicalUnit.ToPayload();
            updateSample.FallbackScreenDpi = canvasScaler.fallbackScreenDPI;
            updateSample.DefaultSpriteDpi = canvasScaler.defaultSpriteDPI;
            updateSample.DynamicPixelsPerUnit = canvasScaler.dynamicPixelsPerUnit;
            _createSamples[objSafeRef] = new CanvasScalerCreate { Id = GetComponentIdentifierPayload(objSafeRef) };
        }

        protected override void OnObjectMarkedDestroyed(CanvasScalerSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedDestroyed(objSafeRef, ctx);
            _destroySamples[objSafeRef] = new CanvasScalerDestroy { Id = GetComponentIdentifierPayload(objSafeRef) };
        }

        private CanvasScalerUpdate GetOrCreateUpdateSample(CanvasScalerSafeRef objSafeRef)
        {
            if (_updateSamples.TryGetValue(objSafeRef, out var sample))
                return sample;
            sample = new CanvasScalerUpdate { Id = GetComponentIdentifierPayload(objSafeRef) };
            _updateSamples[objSafeRef] = sample;
            return sample;
        }

        protected override CanvasScalerFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            var frameData = CanvasScalerFrameData.Pool.Get();
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