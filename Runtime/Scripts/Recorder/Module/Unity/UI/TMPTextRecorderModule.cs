using System.Collections.Generic;
using PLUME.Sample.Unity.UI;
using TMPro;
using UnityEngine;

namespace PLUME.UI
{
    [DisallowMultipleComponent]
    public class TMPTextRecorderModule : RecorderModule,
        IStartRecordingObjectEventReceiver,
        IStopRecordingObjectEventReceiver
    {
        private readonly HashSet<TextMeshProUGUI> _recordedTMPTexts = new();
        
        public void OnStartRecordingObject(Object obj)
        {
            if (obj is TextMeshProUGUI tmpText && !_recordedTMPTexts.Contains(tmpText))
            {
                _recordedTMPTexts.Add(tmpText);
                RecordCreation(tmpText);
            }
        }

        public void OnStopRecordingObject(Object obj)
        {
            if (obj is TextMeshProUGUI tmpText && _recordedTMPTexts.Contains(tmpText))
            {
                _recordedTMPTexts.Remove(tmpText);
                RecordDestruction(tmpText);
            }
        }

        private void RecordCreation(TextMeshProUGUI tmpText)
        {
            var tmpTextCreate = new TMPTextCreate {Id = tmpText.ToIdentifierPayload()};

            var tmpTextUpdateValue = new TMPTextUpdateValue
            {
                Id = tmpText.ToIdentifierPayload(),
                Text = tmpText.text
            };
            var tmpTextUpdateColor = new TMPTextUpdateColor
            {
                Id = tmpText.ToIdentifierPayload(),
                Color = tmpText.color.ToPayload()
            };
            var tmpTextUpdateFont = new TMPTextUpdateFont
            {
                Id = tmpText.ToIdentifierPayload(),
                FontId = tmpText.font.ToAssetIdentifierPayload(),
                FontStyle = (int) tmpText.fontStyle,
                FontSize = tmpText.fontSize,
                AutoSize = tmpText.enableAutoSizing,
                FontSizeMin = tmpText.fontSizeMin,
                FontSizeMax = tmpText.fontSizeMax
            };
            var tmpTextUpdateExtras = new TMPTextUpdateExtras
            {
                Id = tmpText.ToIdentifierPayload(),
                CharacterSpacing = tmpText.characterSpacing,
                WordSpacing = tmpText.wordSpacing,
                LineSpacing = tmpText.lineSpacing,
                ParagraphSpacing = tmpText.paragraphSpacing,
                Alignment = (int) tmpText.alignment,
                WrappingEnabled = tmpText.enableWordWrapping,
                Overflow = (int) tmpText.overflowMode,
                HorizontalMapping = (int) tmpText.horizontalMapping,
                VerticalMapping = (int) tmpText.verticalMapping,
                Margin = tmpText.margin.ToPayload()
            };

            recorder.RecordSample(tmpTextCreate);
            recorder.RecordSample(tmpTextUpdateValue);
            recorder.RecordSample(tmpTextUpdateColor);
            recorder.RecordSample(tmpTextUpdateFont);
            recorder.RecordSample(tmpTextUpdateExtras);
        }

        private void RecordDestruction(TextMeshProUGUI tmpText)
        {
            var tmpTextDestroy = new TMPTextDestroy {Id = tmpText.ToIdentifierPayload()};
            recorder.RecordSample(tmpTextDestroy);
        }
    }
}