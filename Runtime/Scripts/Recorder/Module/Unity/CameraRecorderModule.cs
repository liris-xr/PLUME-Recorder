using System.Collections.Generic;
using PLUME.Sample.Unity;
using UnityEngine;

namespace PLUME
{
    [DisallowMultipleComponent]
    public class CameraRecorderModule : RecorderModule,
        IStartRecordingObjectEventReceiver,
        IStopRecordingObjectEventReceiver
    {
        private readonly HashSet<Camera> _recordedCameras = new();

        public void OnStartRecordingObject(Object obj)
        {
            if (obj is Camera camera && !_recordedCameras.Contains(camera))
            {
                _recordedCameras.Add(camera);
                RecordCreation(camera);
            }
        }

        public void OnStopRecordingObject(Object obj)
        {
            if (obj is Camera camera && _recordedCameras.Contains(camera))
            {
                _recordedCameras.Remove(camera);
                RecordDestruction(camera);
            }
        }

        private void RecordCreation(Camera camera)
        {
            var cameraCreate = new CameraCreate {Id = camera.ToIdentifierPayload()};
            recorder.RecordSample(cameraCreate);
        }

        private void RecordDestruction(Camera camera)
        {
            var cameraDestroy = new CameraDestroy {Id = camera.ToIdentifierPayload()};
            recorder.RecordSample(cameraDestroy);
        }
    }
}