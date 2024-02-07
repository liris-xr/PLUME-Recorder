using System.Collections.Generic;
using PLUME.Sample.Unity;
using UnityEngine;
using UnityRuntimeGuid;

namespace PLUME.Recorder.Module.Unity
{
    [DisallowMultipleComponent]
    public class CameraRecorderModule : RecorderModule,
        IStartRecordingObjectEventReceiver,
        IStopRecordingObjectEventReceiver
    {
        private readonly Dictionary<int, Camera> _recordedCameras = new();

        public void OnStartRecordingObject(Object obj)
        {
            if (obj is Camera cam && !_recordedCameras.ContainsKey(cam.GetInstanceID()))
            {
                _recordedCameras.Add(cam.GetInstanceID(), cam);
                RecordCreation(cam);
            }
        }

        public void OnStopRecordingObject(int objectInstanceId)
        {
            if (_recordedCameras.ContainsKey(objectInstanceId))
            {
                RecordDestruction(objectInstanceId);
                RemoveFromCache(objectInstanceId);
            }
        }

        private void RemoveFromCache(int cameraInstanceId)
        {
            _recordedCameras.Remove(cameraInstanceId);
        }

        private void RecordCreation(Camera camera)
        {
            var identifier = camera.ToIdentifierPayload();
            var cameraCreate = new CameraCreate { Id = identifier };
            var cameraUpdate = new CameraUpdate
            {
                Id = identifier,
                NearClipPlane = camera.nearClipPlane,
                FarClipPlane = camera.farClipPlane,
                FieldOfView = camera.fieldOfView,
                RenderingPath = camera.renderingPath.ToPayload(),
                AllowHdr = camera.allowHDR,
                AllowMsaa = camera.allowMSAA,
                AllowDynamicResolution = camera.allowDynamicResolution,
                ForceIntoRenderTexture = camera.forceIntoRenderTexture,
                OrthographicSize = camera.orthographicSize,
                Orthographic = camera.orthographic,
                OpaqueSortMode = camera.opaqueSortMode.ToPayload(),
                TransparencySortMode = camera.transparencySortMode.ToPayload(),
                TransparencySortAxis = camera.transparencySortAxis.ToPayload(),
                Depth = camera.depth,
                Aspect = camera.aspect,
                CullingMask = camera.cullingMask,
                EventMask = camera.eventMask,
                LayerCullSpherical = camera.layerCullSpherical,
                CameraType = camera.cameraType.ToPayload(),
                UseOcclusionCulling = camera.useOcclusionCulling,
                CullingMatrix = camera.cullingMatrix.ToPayload(),
                BackgroundColor = camera.backgroundColor.ToPayload(),
                ClearFlags = camera.clearFlags.ToPayload(),
                DepthTextureMode = camera.depthTextureMode.ToPayload(),
                ClearStencilAfterLightingPass = camera.clearStencilAfterLightingPass,
                UsePhysicalProperties = camera.usePhysicalProperties,
                SensorSize = camera.sensorSize.ToPayload(),
                LensShift = camera.lensShift.ToPayload(),
                FocalLength = camera.focalLength,
                GateFit = camera.gateFit.ToPayload(),
                Rect = camera.rect.ToPayload(),
                PixelRect = camera.pixelRect.ToPayload(),
                TargetTextureId = camera.targetTexture == null ? "" : SceneGuidRegistry.GetOrCreate(camera.gameObject.scene).GetOrCreateEntry(camera.targetTexture).guid,
                TargetDisplay = camera.targetDisplay,
                WorldToCameraMatrix = camera.worldToCameraMatrix.ToPayload(),
                ProjectionMatrix = camera.projectionMatrix.ToPayload(),
                NonJitteredProjectionMatrix = camera.nonJitteredProjectionMatrix.ToPayload(),
                UseJitteredProjectionMatrixForTransparentRendering =
                    camera.useJitteredProjectionMatrixForTransparentRendering,
                SceneIdx = camera.scene.buildIndex,
                StereoSeparation = camera.stereoSeparation,
                StereoConvergence = camera.stereoConvergence,
                StereoTargetEye = camera.stereoTargetEye.ToPayload()
            };
            
            cameraUpdate.LayerCullDistances.AddRange(camera.layerCullDistances);

            recorder.RecordSampleStamped(cameraCreate);
            recorder.RecordSampleStamped(cameraUpdate);
        }

        private void RecordDestruction(int cameraInstanceId)
        {
            var cameraDestroy = new ComponentDestroy
                { Id = new ComponentDestroyIdentifier { Id = cameraInstanceId.ToString() } };
            recorder.RecordSampleStamped(cameraDestroy);
        }

        protected override void ResetCache()
        {
            _recordedCameras.Clear();
        }
    }
}