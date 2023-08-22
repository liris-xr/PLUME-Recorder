using System.Collections.Generic;
using PLUME.Sample.Unity.UI;
using UnityEngine;

namespace PLUME.UI
{
    [DisallowMultipleComponent]
    public class CanvasRecorderModule : RecorderModule,
        IStartRecordingObjectEventReceiver,
        IStopRecordingObjectEventReceiver
    {
        private readonly HashSet<Canvas> _recordedCanvas = new();

        public void OnStartRecordingObject(Object obj)
        {
            if (obj is Canvas canvas && !_recordedCanvas.Contains(canvas))
            {
                _recordedCanvas.Add(canvas);
                RecordCreation(canvas);
            }
        }

        public void OnStopRecordingObject(Object obj)
        {
            if (obj is Canvas canvas && _recordedCanvas.Contains(canvas))
            {
                _recordedCanvas.Remove(canvas);
                RecordDestruction(canvas);
            }
        }

        private void RecordCreation(Canvas canvas)
        {
            var canvasCreate = new CanvasCreate {Id = canvas.ToIdentifierPayload()};
            var canvasUpdate = new CanvasUpdateRenderMode()
            {
                Id = canvas.ToIdentifierPayload(),
                RenderMode = (int) canvas.renderMode
            };

            recorder.RecordSample(canvasCreate);
            recorder.RecordSample(canvasUpdate);
        }

        private void RecordDestruction(Canvas canvas)
        {
            var canvasDestroy = new CanvasDestroy {Id = canvas.ToIdentifierPayload()};
            recorder.RecordSample(canvasDestroy);
        }
    }
}