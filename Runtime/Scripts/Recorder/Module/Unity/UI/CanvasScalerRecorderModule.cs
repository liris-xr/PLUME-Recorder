using System.Collections.Generic;
using PLUME.Sample.Unity;
using PLUME.Sample.Unity.UI;
using UnityEngine;
using UnityEngine.UI;

namespace PLUME.UI
{
    [DisallowMultipleComponent]
    public class CanvasScalerRecorderModule : RecorderModule,
        IStartRecordingObjectEventReceiver,
        IStopRecordingObjectEventReceiver
    {
        private readonly Dictionary<int, CanvasScaler> _recordedCanvasScaler = new();

        public void OnStartRecordingObject(Object obj)
        {
            if (obj is CanvasScaler canvasScaler && !_recordedCanvasScaler.ContainsKey(canvasScaler.GetInstanceID()))
            {
                _recordedCanvasScaler.Add(canvasScaler.GetInstanceID(), canvasScaler);
                RecordCreation(canvasScaler);
            }
        }

        public void OnStopRecordingObject(int objectInstanceId)
        {
            if (_recordedCanvasScaler.ContainsKey(objectInstanceId))
            {
                RecordDestruction(objectInstanceId);
                RemoveFromCache(objectInstanceId);
            }
        }

        private void RemoveFromCache(int canvasScalerInstanceId)
        {
            _recordedCanvasScaler.Remove(canvasScalerInstanceId);
        }

        private void RecordCreation(CanvasScaler canvasScaler)
        {
            var canvasScalerCreate = new CanvasScalerCreate {Id = canvasScaler.ToIdentifierPayload()};
            var canvasScalerUpdate = new CanvasScalerUpdatePixelsPerUnit()
            {
                Id = canvasScaler.ToIdentifierPayload(),
                DynamicPixelsPerUnit = canvasScaler.dynamicPixelsPerUnit,
                ReferencePixelsPerUnit = canvasScaler.referencePixelsPerUnit,
            };

            recorder.RecordSample(canvasScalerCreate);
            recorder.RecordSample(canvasScalerUpdate);
        }

        private void RecordDestruction(int canvasScalerInstanceId)
        {
            var canvasDestroy = new ComponentDestroy
                {Id = new ComponentDestroyIdentifier {Id = canvasScalerInstanceId.ToString()}};
            recorder.RecordSample(canvasDestroy);
        }

        protected override void ResetCache()
        {
            _recordedCanvasScaler.Clear();
        }
    }
}