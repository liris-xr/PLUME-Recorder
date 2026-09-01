#if HDRP_ENABLED
using System.Collections.Generic;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.HDRP;
using UnityEngine.Scripting;
using HDAdditionalCameraDataSafeRef =
    PLUME.Core.Object.SafeRef.IComponentSafeRef<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>;
using static PLUME.Core.Utils.SampleUtils;

namespace PLUME.Base.Module.Unity.HDRP
{
    [Preserve]
    public class HDAdditionalCameraDataRecorderModule : ComponentRecorderModule<
        UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData, HDAdditionalCameraDataFrameData>
    {
        private readonly Dictionary<HDAdditionalCameraDataSafeRef, HDAdditionalCameraDataCreate> _createSamples =
            new();

        private readonly Dictionary<HDAdditionalCameraDataSafeRef, HDAdditionalCameraDataDestroy> _destroySamples =
            new();

        private readonly Dictionary<HDAdditionalCameraDataSafeRef, HDAdditionalCameraDataUpdate> _updateSamples =
            new();

        protected override void OnObjectMarkedCreated(HDAdditionalCameraDataSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);

            var cameraData = objSafeRef.Component;

            // HDAdditionalCameraData requires a sibling Camera component. Physical camera / exposure
            // properties (iso, shutterSpeed, aperture, focusDistance) used to live on
            // HDAdditionalCameraData.physicalParameters, but that struct is obsolete
            // ("Properties have been migrated to Camera class. #from(2022.2)") and HDRP itself now reads
            // them from the core UnityEngine.Camera component (see HDRenderPipeline.PostProcess.cs /
            // PathTracing.cs, which use hdCamera.camera.iso / .aperture / .shutterSpeed / .focusDistance).
            var camera = cameraData.GetComponent<UnityEngine.Camera>();

            var volumeAnchorOverrideSafeRef = ctx.SafeRefProvider.GetOrCreateComponentSafeRef(
                cameraData.volumeAnchorOverride);

            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.ClearColorMode = cameraData.clearColorMode.ToPayload();
            updateSample.BackgroundColorHdr = cameraData.backgroundColorHDR.ToPayload();
            updateSample.ClearDepth = cameraData.clearDepth;
            updateSample.VolumeLayerMask = cameraData.volumeLayerMask.value;
            updateSample.VolumeAnchorOverride = GetComponentIdentifierPayload(volumeAnchorOverrideSafeRef);
            updateSample.Antialiasing = cameraData.antialiasing.ToPayload();
            updateSample.SmaaQuality = cameraData.SMAAQuality.ToPayload();
            updateSample.TaaQuality = cameraData.TAAQuality.ToPayload();
            updateSample.Dithering = cameraData.dithering;
            updateSample.StopNans = cameraData.stopNaNs;
            updateSample.CustomRenderingSettings = cameraData.customRenderingSettings;
            updateSample.XrRendering = cameraData.xrRendering;

            if (camera != null)
            {
                updateSample.FocusDistance = camera.focusDistance;
                updateSample.Iso = camera.iso;
                updateSample.ShutterSpeed = camera.shutterSpeed;
                updateSample.Aperture = camera.aperture;
            }

            _createSamples[objSafeRef] = new HDAdditionalCameraDataCreate
            {
                Component = GetComponentIdentifierPayload(objSafeRef)
            };
        }

        protected override void OnObjectMarkedDestroyed(HDAdditionalCameraDataSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedDestroyed(objSafeRef, ctx);
            _destroySamples[objSafeRef] =
                new HDAdditionalCameraDataDestroy { Component = GetComponentIdentifierPayload(objSafeRef) };
        }

        private HDAdditionalCameraDataUpdate GetOrCreateUpdateSample(HDAdditionalCameraDataSafeRef objSafeRef)
        {
            if (_updateSamples.TryGetValue(objSafeRef, out var sample))
                return sample;
            sample = new HDAdditionalCameraDataUpdate { Component = GetComponentIdentifierPayload(objSafeRef) };
            _updateSamples[objSafeRef] = sample;
            return sample;
        }

        protected override HDAdditionalCameraDataFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            var frameData = HDAdditionalCameraDataFrameData.Pool.Get();
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
#endif
