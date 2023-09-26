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
            var cameraCreate = new CameraCreate {Id = camera.ToIdentifierPayload()};
            recorder.RecordSampleStamped(cameraCreate);
        }

        private void RecordDestruction(int cameraInstanceId)
        {
            var cameraDestroy = new ComponentDestroy
                {Id = new ComponentDestroyIdentifier {Id = cameraInstanceId.ToString()}};
            recorder.RecordSampleStamped(cameraDestroy);
        }

        protected override void ResetCache()
        {
            _recordedCameras.Clear();
        }
    }
}