using System.Collections.Generic;
using PLUME.Sample.Unity;
using PLUME.Sample.Unity.URP;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace PLUME.Recorder.Module.Unity.URP
{
#if !URP_ENABLED
    public class AdditionalCameraDataRecorderModule : RecorderModule
    {
        protected override void ResetCache() {}
    }
#else
    public class AdditionalCameraDataRecorderModule : RecorderModule,
        IStartRecordingObjectEventReceiver,
        IStopRecordingObjectEventReceiver
    {
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
                RequiresDepthTexture = camData.requiresDepthTexture,
                RequiresColorTexture = camData.requiresColorTexture,
                VolumeLayerMask = camData.volumeLayerMask,
                VolumeTriggerId = camData.volumeTrigger == null ? null : camData.volumeTrigger.ToIdentifierPayload(),
                RenderPostProcessing = camData.renderPostProcessing,
                Antialiasing = camData.antialiasing.ToPayload(),
                AntialiasingQuality = camData.antialiasingQuality.ToPayload(),
                StopNan = camData.stopNaN,
                Dithering = camData.dithering,
                AllowXrRendering = camData.allowXRRendering
            };

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
    }
#endif
}