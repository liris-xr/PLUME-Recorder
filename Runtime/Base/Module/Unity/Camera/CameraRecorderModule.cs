using System.Collections.Generic;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Core.Utils;
using PLUME.Sample.Unity;
using UnityEngine.Scripting;
using CameraSafeRef = PLUME.Core.Object.SafeRef.IComponentSafeRef<UnityEngine.Camera>;

namespace PLUME.Base.Module.Unity.Camera
{
    [Preserve]
    public class CameraRecorderModule : ComponentRecorderModule<UnityEngine.Camera, CameraFrameData>
    {
        private readonly Dictionary<CameraSafeRef, CameraCreate> _createSamples = new();
        private readonly Dictionary<CameraSafeRef, CameraDestroy> _destroySamples = new();
        private readonly Dictionary<CameraSafeRef, CameraUpdate> _updateSamples = new();

        protected override void OnCreate(RecorderContext ctx)
        {
            base.OnCreate(ctx);
        }

        protected override void OnObjectMarkedCreated(CameraSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);

            var targetTextureSafeRef =
                ctx.ObjectSafeRefProvider.GetOrCreateAssetSafeRef(objSafeRef.Component.targetTexture);

            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.NearClipPlane = objSafeRef.Component.nearClipPlane;
            updateSample.FarClipPlane = objSafeRef.Component.farClipPlane;
            updateSample.FieldOfView = objSafeRef.Component.fieldOfView;
            updateSample.RenderingPath = objSafeRef.Component.renderingPath.ToPayload();
            updateSample.AllowHdr = objSafeRef.Component.allowHDR;
            updateSample.AllowMsaa = objSafeRef.Component.allowMSAA;
            updateSample.AllowDynamicResolution = objSafeRef.Component.allowDynamicResolution;
            updateSample.ForceIntoRenderTexture = objSafeRef.Component.forceIntoRenderTexture;
            updateSample.Orthographic = objSafeRef.Component.orthographic;
            updateSample.OrthographicSize = objSafeRef.Component.orthographicSize;
            updateSample.OpaqueSortMode = objSafeRef.Component.opaqueSortMode.ToPayload();
            updateSample.TransparencySortMode = objSafeRef.Component.transparencySortMode.ToPayload();
            updateSample.TransparencySortAxis = objSafeRef.Component.transparencySortAxis.ToPayload();
            updateSample.Depth = objSafeRef.Component.depth;
            updateSample.Aspect = objSafeRef.Component.aspect;
            updateSample.CullingMask = objSafeRef.Component.cullingMask;
            updateSample.EventMask = objSafeRef.Component.eventMask;
            updateSample.LayerCullSpherical = objSafeRef.Component.layerCullSpherical;
            updateSample.CameraType = objSafeRef.Component.cameraType.ToPayload();
            updateSample.LayerCullDistances = new CameraLayerCullDistances();
            updateSample.LayerCullDistances.Distances.AddRange(objSafeRef.Component.layerCullDistances);
            updateSample.UseOcclusionCulling = objSafeRef.Component.useOcclusionCulling;
            updateSample.CullingMatrix = objSafeRef.Component.cullingMatrix.ToPayload();
            updateSample.BackgroundColor = objSafeRef.Component.backgroundColor.ToPayload();
            updateSample.ClearFlags = objSafeRef.Component.clearFlags.ToPayload();
            updateSample.DepthTextureMode = objSafeRef.Component.depthTextureMode.ToPayload();
            updateSample.ClearStencilAfterLightingPass = objSafeRef.Component.clearStencilAfterLightingPass;
            updateSample.UsePhysicalProperties = objSafeRef.Component.usePhysicalProperties;
            updateSample.SensorSize = objSafeRef.Component.sensorSize.ToPayload();
            updateSample.LensShift = objSafeRef.Component.lensShift.ToPayload();
            updateSample.FocalLength = objSafeRef.Component.focalLength;
            updateSample.GateFit = objSafeRef.Component.gateFit.ToPayload();
            updateSample.Rect = objSafeRef.Component.rect.ToPayload();
            updateSample.PixelRect = objSafeRef.Component.pixelRect.ToPayload();
            updateSample.TargetTextureId = targetTextureSafeRef.ToAssetIdentifierPayload();
            updateSample.TargetDisplay = objSafeRef.Component.targetDisplay;
            updateSample.WorldToCameraMatrix = objSafeRef.Component.worldToCameraMatrix.ToPayload();
            updateSample.ProjectionMatrix = objSafeRef.Component.projectionMatrix.ToPayload();
            updateSample.NonJitteredProjectionMatrix = objSafeRef.Component.nonJitteredProjectionMatrix.ToPayload();
            updateSample.UseJitteredProjectionMatrixForTransparentRendering =
                objSafeRef.Component.useJitteredProjectionMatrixForTransparentRendering;
            updateSample.StereoSeparation = objSafeRef.Component.stereoSeparation;
            updateSample.StereoConvergence = objSafeRef.Component.stereoConvergence;
            updateSample.StereoTargetEye = objSafeRef.Component.stereoTargetEye.ToPayload();
            _createSamples[objSafeRef] = new CameraCreate { Id = objSafeRef.ToIdentifierPayload() };
        }

        protected override void OnObjectMarkedDestroyed(CameraSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedDestroyed(objSafeRef, ctx);
            _destroySamples[objSafeRef] = new CameraDestroy { Id = objSafeRef.ToIdentifierPayload() };
        }

        private CameraUpdate GetOrCreateUpdateSample(CameraSafeRef objSafeRef)
        {
            if (_updateSamples.TryGetValue(objSafeRef, out var sample))
                return sample;
            sample = new CameraUpdate { Id = objSafeRef.ToIdentifierPayload() };
            _updateSamples[objSafeRef] = sample;
            return sample;
        }

        protected override CameraFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            var frameData = CameraFrameData.Pool.Get();
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