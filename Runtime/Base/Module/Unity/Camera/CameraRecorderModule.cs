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

        protected override void OnObjectMarkedCreated(CameraSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);
            
            var camera = objSafeRef.Component;
            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.NearClipPlane = camera.nearClipPlane;
            updateSample.FarClipPlane = camera.farClipPlane;
            updateSample.FieldOfView = camera.fieldOfView;
            updateSample.RenderingPath = camera.renderingPath.ToPayload();
            updateSample.AllowHdr = camera.allowHDR;
            updateSample.AllowMsaa = camera.allowMSAA;
            updateSample.AllowDynamicResolution = camera.allowDynamicResolution;
            updateSample.ForceIntoRenderTexture = camera.forceIntoRenderTexture;
            updateSample.Orthographic = camera.orthographic;
            updateSample.OrthographicSize = camera.orthographicSize;
            updateSample.OpaqueSortMode = camera.opaqueSortMode.ToPayload();
            updateSample.TransparencySortMode = camera.transparencySortMode.ToPayload();
            updateSample.TransparencySortAxis = camera.transparencySortAxis.ToPayload();
            updateSample.Depth = camera.depth;
            updateSample.Aspect = camera.aspect;
            updateSample.CullingMask = camera.cullingMask;
            updateSample.EventMask = camera.eventMask;
            updateSample.LayerCullSpherical = camera.layerCullSpherical;
            updateSample.CameraType = camera.cameraType.ToPayload();
            updateSample.LayerCullDistances = new CameraLayerCullDistances();
            updateSample.LayerCullDistances.Distances.AddRange(camera.layerCullDistances);
            updateSample.UseOcclusionCulling = camera.useOcclusionCulling;
            updateSample.CullingMatrix = camera.cullingMatrix.ToPayload();
            updateSample.BackgroundColor = camera.backgroundColor.ToPayload();
            updateSample.ClearFlags = camera.clearFlags.ToPayload();
            updateSample.DepthTextureMode = camera.depthTextureMode.ToPayload();
            updateSample.ClearStencilAfterLightingPass = camera.clearStencilAfterLightingPass;
            updateSample.UsePhysicalProperties = camera.usePhysicalProperties;
            updateSample.SensorSize = camera.sensorSize.ToPayload();
            updateSample.LensShift = camera.lensShift.ToPayload();
            updateSample.FocalLength = camera.focalLength;
            updateSample.GateFit = camera.gateFit.ToPayload();
            updateSample.Rect = camera.rect.ToPayload();
            updateSample.PixelRect = camera.pixelRect.ToPayload();
            updateSample.TargetTextureId = camera.targetTexture.ToAssetIdentifierPayload();
            updateSample.TargetDisplay = camera.targetDisplay;
            updateSample.WorldToCameraMatrix = camera.worldToCameraMatrix.ToPayload();
            updateSample.ProjectionMatrix = camera.projectionMatrix.ToPayload();
            updateSample.NonJitteredProjectionMatrix = camera.nonJitteredProjectionMatrix.ToPayload();
            updateSample.UseJitteredProjectionMatrixForTransparentRendering =
                camera.useJitteredProjectionMatrixForTransparentRendering;
            updateSample.StereoSeparation = camera.stereoSeparation;
            updateSample.StereoConvergence = camera.stereoConvergence;
            updateSample.StereoTargetEye = camera.stereoTargetEye.ToPayload();
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