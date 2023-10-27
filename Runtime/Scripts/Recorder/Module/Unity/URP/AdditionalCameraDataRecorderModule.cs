using System.Collections.Generic;
using System.Linq;
using PLUME.Sample.Unity;
using PLUME.Sample.Unity.URP;
using UnityEngine;
#if URP_ENABLED
using UnityEngine.Rendering.Universal;
#endif

namespace PLUME.URP
{
    public class AdditionalCameraDataRecorderModule : RecorderModule,
        IStartRecordingObjectEventReceiver,
        IStopRecordingObjectEventReceiver
    {
#if URP_ENABLED
        private readonly Dictionary<int, UniversalAdditionalCameraData> _recordedAdditionalCameraData = new();

        public void OnStartRecordingObject(Object obj)
        {
            if (obj is UniversalAdditionalCameraData camData &&
                !_recordedAdditionalCameraData.ContainsKey(camData.GetInstanceID()))
            {
                _recordedAdditionalCameraData.Add(camData.GetInstanceID(), camData);
                RecordCreation(camData);
            }
        }

        public void OnStopRecordingObject(int objectInstanceId)
        {
            if (_recordedAdditionalCameraData.ContainsKey(objectInstanceId))
            {
                RecordDestruction(objectInstanceId);
                RemoveFromCache(objectInstanceId);
            }
        }

        private void RemoveFromCache(int cameraInstanceId)
        {
            _recordedAdditionalCameraData.Remove(cameraInstanceId);
        }

        private void RecordCreation(UniversalAdditionalCameraData camData)
        {
            var identifier = camData.ToIdentifierPayload();
            var cameraDataCreate = new AdditionalCameraDataCreate { Id = identifier };
            var cameraDataUpdate = new AdditionalCameraDataUpdate
            {
                Id = identifier,
                Version = camData.version,
                RenderShadows = camData.renderShadows,
                RequiresDepthOption = camData.requiresDepthOption.ToPayload(),
                RequiresColorOption = camData.requiresColorOption.ToPayload(),
                RenderType = camData.renderType.ToPayload(),
                ClearDepth = camData.clearDepth,
                RequiresDepthTexture = camData.requiresDepthTexture,
                RequiresColorTexture = camData.requiresColorTexture,
                VolumeLayerMask = camData.volumeLayerMask,
                VolumeTrigger = camData.volumeTrigger.ToIdentifierPayload(),
                RequiresVolumeFrameworkUpdate = camData.requiresVolumeFrameworkUpdate,
                // VolumeStack = camData.volumeStack,
                RenderPostProcessing = camData.renderPostProcessing,
                Antialiasing = camData.antialiasing.ToPayload(),
                AntialiasingQuality = camData.antialiasingQuality.ToPayload(),
                StopNan = camData.stopNaN,
                Dithering = camData.dithering,
                AllowXrRendering = camData.allowXRRendering
            };
            
            cameraDataUpdate.CameraStackIds.AddRange(camData.cameraStack.Select(cam => cam.ToIdentifierPayload()));
            
            recorder.RecordSampleStamped(cameraDataCreate);
            recorder.RecordSampleStamped(cameraDataUpdate);
        }

        private void RecordDestruction(int cameraInstanceId)
        {
            var cameraDestroy = new ComponentDestroy
                { Id = new ComponentDestroyIdentifier { Id = cameraInstanceId.ToString() } };
            recorder.RecordSampleStamped(cameraDestroy);
        }

        protected override void ResetCache()
        {
            _recordedAdditionalCameraData.Clear();
        }

#else
        protected override void ResetCache()
        {
        }

        public void OnStartRecordingObject(Object obj)
        {
        }

        public void OnStopRecordingObject(int objectInstanceId)
        {
        }
#endif
    }
}