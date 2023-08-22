using System.Collections.Generic;
using PLUME.Sample.Unity.UI;
using UnityEngine;
using UnityEngine.UI;
using Vector4 = PLUME.Sample.Common.Vector4;

namespace PLUME.UI
{
    [DisallowMultipleComponent]
    public class ImageRecorderModule : RecorderModule,
        IStartRecordingObjectEventReceiver,
        IStopRecordingObjectEventReceiver
    {
        private readonly HashSet<Image> _recordedImages = new();

        public void OnStartRecordingObject(Object obj)
        {
            if (obj is Image image && !_recordedImages.Contains(image))
            {
                _recordedImages.Add(image);
                RecordCreation(image);
            }
        }

        public void OnStopRecordingObject(Object obj)
        {
            if (obj is Image image && _recordedImages.Contains(image))
            {
                _recordedImages.Remove(image);
                RecordDestruction(image);
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

            recorder.RecordSample(imageCreate);
            recorder.RecordSample(imageUpdateColor);
            recorder.RecordSample(imageUpdateSprite);
        }

        private void RecordDestruction(Image image)
        {
            var imageDestroy = new ImageDestroy {Id = image.ToIdentifierPayload()};
            recorder.RecordSample(imageDestroy);
        }
    }
}