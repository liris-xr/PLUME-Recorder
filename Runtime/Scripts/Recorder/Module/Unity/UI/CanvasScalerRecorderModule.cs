using System.Collections.Generic;
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
        private readonly HashSet<CanvasScaler> _recordedCanvasScaler = new();

        public void OnStartRecordingObject(Object obj)
        {
            if (obj is CanvasScaler canvasScaler && !_recordedCanvasScaler.Contains(canvasScaler))
            {
                _recordedCanvasScaler.Add(canvasScaler);
                RecordCreation(canvasScaler);
            }
        }

        public void OnStopRecordingObject(Object obj)
        {
            if (obj is CanvasScaler canvasScaler && _recordedCanvasScaler.Contains(canvasScaler))
            {
                _recordedCanvasScaler.Remove(canvasScaler);
                RecordDestruction(canvasScaler);
            }
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

        private void RecordDestruction(CanvasScaler canvasScaler)
        {
            var canvasDestroy = new CanvasScalerDestroy {Id = canvasScaler.ToIdentifierPayload()};
            recorder.RecordSample(canvasDestroy);
        }
    }
}