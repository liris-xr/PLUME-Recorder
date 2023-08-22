using System.Collections.Generic;
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
        private readonly HashSet<Text> _recordedTexts = new();

        public void OnStartRecordingObject(Object obj)
        {
            if (obj is Text text && !_recordedTexts.Contains(text))
            {
                _recordedTexts.Add(text);
                RecordCreation(text);
            }
        }

        public void OnStopRecordingObject(Object obj)
        {
            if (obj is Text text && _recordedTexts.Contains(text))
            {
                _recordedTexts.Remove(text);
                RecordDestruction(text);
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

            recorder.RecordSample(textCreate);
            recorder.RecordSample(textUpdateValue);
            recorder.RecordSample(textUpdateFont);
            recorder.RecordSample(textUpdateColor);
            recorder.RecordSample(textUpdateExtras);
        }

        private void RecordDestruction(Text text)
        {
            var textDestroy = new TextDestroy {Id = text.ToIdentifierPayload()};
            recorder.RecordSample(textDestroy);
        }
    }
}