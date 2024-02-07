using System.Collections.Generic;
using PLUME.Sample.Unity;
using PLUME.Sample.Unity.UI;
using UnityEngine;
using UnityEngine.UI;
using Vector4 = PLUME.Sample.Common.Vector4;

namespace PLUME.Recorder.Module.Unity.UI
{
    [DisallowMultipleComponent]
    public class ImageRecorderModule : RecorderModule,
        IStartRecordingObjectEventReceiver,
        IStopRecordingObjectEventReceiver
    {
        private readonly Dictionary<int, Image> _recordedImages = new();

        public void OnStartRecordingObject(Object obj)
        {
            if (obj is Image image && !_recordedImages.ContainsKey(image.GetInstanceID()))
            {
                _recordedImages.Add(image.GetInstanceID(), image);
                RecordCreation(image);
            }
        }

        public void OnStopRecordingObject(int objectInstanceId)
        {
            if (_recordedImages.ContainsKey(objectInstanceId))
            {
                RecordDestruction(objectInstanceId);
                RemoveFromCache(objectInstanceId);
            }
        }

        private void RecordCreation(Image image)
        {
            var imageCreate = new ImageCreate {Id = image.ToIdentifierPayload()};
            var imageUpdateColor = new ImageUpdateColor
            {
                Id = image.ToIdentifierPayload(),
                Color = new Vector4 {X = image.color.r, Y = image.color.g, Z = image.color.b, W = image.color.a}
            };
            var imageUpdateSprite = new ImageUpdateSprite
            {
                Id = image.ToIdentifierPayload(),
                SpriteId = image.sprite.ToAssetIdentifierPayload(),
            };

            recorder.RecordSampleStamped(imageCreate);
            recorder.RecordSampleStamped(imageUpdateColor);
            recorder.RecordSampleStamped(imageUpdateSprite);
        }

        private void RemoveFromCache(int imageInstanceId)
        {
            _recordedImages.Remove(imageInstanceId);
        }

        private void RecordDestruction(int imageInstanceId)
        {
            var imageDestroy = new ComponentDestroy
                {Id = new ComponentDestroyIdentifier {Id = imageInstanceId.ToString()}};
            recorder.RecordSampleStamped(imageDestroy);
        }

        protected override void ResetCache()
        {
            _recordedImages.Clear();
        }
    }
}