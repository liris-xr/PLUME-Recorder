using System.Collections.Generic;
using PLUME.Sample.Unity;
using PLUME.Sample.Unity.UI;
using UnityEngine;
using UnityEngine.UI;

namespace PLUME.UI
{
    [DisallowMultipleComponent]
    public class TextRecorderModule : RecorderModule,
        IStartRecordingObjectEventReceiver,
        IStopRecordingObjectEventReceiver
    {
        private readonly Dictionary<int, Text> _recordedTexts = new();

        public void OnStartRecordingObject(Object obj)
        {
            if (obj is Text text && !_recordedTexts.ContainsKey(text.GetInstanceID()))
            {
                _recordedTexts.Add(text.GetInstanceID(), text);
                RecordCreation(text);
            }
        }

        public void OnStopRecordingObject(int objectInstanceId)
        {
            if (_recordedTexts.ContainsKey(objectInstanceId))
            {
                RecordDestruction(objectInstanceId);
                RemoveFromCache(objectInstanceId);
            }
        }

        private void RecordCreation(Text text)
        {
            var textCreate = new TextCreate {Id = text.ToIdentifierPayload()};
            var textUpdateValue = new TextUpdateValue
            {
                Id = text.ToIdentifierPayload(),
                Text = text.text
            };
            var textUpdateFont = new TextUpdateFont
            {
                Id = text.ToIdentifierPayload(),
                FontId = text.font.ToAssetIdentifierPayload(),
                FontStyle = (int) text.fontStyle,
                FontSize = text.fontSize
            };
            var textUpdateColor = new TextUpdateColor
            {
                Id = text.ToIdentifierPayload(),
                Color = text.color.ToPayload()
            };
            var textUpdateExtras = new TextUpdateExtras
            {
                Id = text.ToIdentifierPayload(),
                LineSpacing = text.lineSpacing,
                SupportRichText = text.supportRichText,
                Alignment = (int) text.alignment,
                AlignByGeometry = text.alignByGeometry,
                HorizontalOverflow = (int) text.horizontalOverflow,
                VerticalOverflow = (int) text.verticalOverflow,
                ResizeTextForBestFit = text.resizeTextForBestFit,
                ResizeTextMinSize = text.resizeTextMinSize,
                ResizeTextMaxSize = text.resizeTextMaxSize,
            };

            recorder.RecordSampleStamped(textCreate);
            recorder.RecordSampleStamped(textUpdateValue);
            recorder.RecordSampleStamped(textUpdateFont);
            recorder.RecordSampleStamped(textUpdateColor);
            recorder.RecordSampleStamped(textUpdateExtras);
        }

        private void RemoveFromCache(int textInstanceId)
        {
            _recordedTexts.Remove(textInstanceId);
        }

        private void RecordDestruction(int textInstanceId)
        {
            var textDestroy = new ComponentDestroy
                {Id = new ComponentDestroyIdentifier {Id = textInstanceId.ToString()}};
            recorder.RecordSampleStamped(textDestroy);
        }

        protected override void ResetCache()
        {
            _recordedTexts.Clear();
        }
    }
}