using System.Collections.Generic;
using PLUME.Sample.Unity;
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
        private readonly Dictionary<int, TextMeshProUGUI> _recordedTMPTexts = new();

        public void OnStartRecordingObject(Object obj)
        {
            if (obj is TextMeshProUGUI tmpText && !_recordedTMPTexts.ContainsKey(tmpText.GetInstanceID()))
            {
                _recordedTMPTexts.Add(tmpText.GetInstanceID(), tmpText);
                RecordCreation(tmpText);
            }
        }

        public void OnStopRecordingObject(int objectInstanceId)
        {
            if (_recordedTMPTexts.ContainsKey(objectInstanceId))
            {
                RecordDestruction(objectInstanceId);
                RemoveFromCache(objectInstanceId);
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

            recorder.RecordSampleStamped(tmpTextCreate);
            recorder.RecordSampleStamped(tmpTextUpdateValue);
            recorder.RecordSampleStamped(tmpTextUpdateColor);
            recorder.RecordSampleStamped(tmpTextUpdateFont);
            recorder.RecordSampleStamped(tmpTextUpdateExtras);
        }

        private void RemoveFromCache(int tmpTextInstanceId)
        {
            _recordedTMPTexts.Remove(tmpTextInstanceId);
        }

        private void RecordDestruction(int tmpTextInstanceId)
        {
            var tmpTextDestroy = new ComponentDestroy
                {Id = new ComponentDestroyIdentifier {Id = tmpTextInstanceId.ToString()}};
            recorder.RecordSampleStamped(tmpTextDestroy);
        }

        protected override void ResetCache()
        {
            _recordedTMPTexts.Clear();
        }
    }
}