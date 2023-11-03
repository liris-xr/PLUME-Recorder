using System.Collections.Generic;
using PLUME.Sample.Unity;
using PLUME.Sample.Unity.UI;
using UnityEngine;

namespace PLUME.UI
{
    [DisallowMultipleComponent]
    public class CanvasRecorderModule : RecorderModule,
        IStartRecordingObjectEventReceiver,
        IStopRecordingObjectEventReceiver
    {
        private readonly Dictionary<int, Canvas> _recordedCanvas = new();

        public void OnStartRecordingObject(Object obj)
        {
            if (obj is Canvas canvas && !_recordedCanvas.ContainsKey(canvas.GetInstanceID()))
            {
                _recordedCanvas.Add(canvas.GetInstanceID(), canvas);
                RecordCreation(canvas);
            }
        }

        public void OnStopRecordingObject(int objectInstanceId)
        {
            if (_recordedCanvas.ContainsKey(objectInstanceId))
            {
                RecordDestruction(objectInstanceId);
                RemoveFromCache(objectInstanceId);
            }
        }

        private void RemoveFromCache(int canvasInstanceId)
        {
            _recordedCanvas.Remove(canvasInstanceId);
        }

        private void RecordCreation(Canvas canvas)
        {
            var canvasCreate = new CanvasCreate {Id = canvas.ToIdentifierPayload()};
            var canvasUpdate = new CanvasUpdateRenderMode()
            {
                Id = canvas.ToIdentifierPayload(),
                RenderMode = (int) canvas.renderMode
            };

            recorder.RecordSampleStamped(canvasCreate);
            recorder.RecordSampleStamped(canvasUpdate);
        }

        private void RecordDestruction(int canvasInstanceId)
        {
            var canvasDestroy = new ComponentDestroy {Id = new ComponentDestroyIdentifier { Id = canvasInstanceId.ToString() }};
            recorder.RecordSampleStamped(canvasDestroy);
        }

        protected override void ResetCache()
        {
            _recordedCanvas.Clear();
        }
    }
}