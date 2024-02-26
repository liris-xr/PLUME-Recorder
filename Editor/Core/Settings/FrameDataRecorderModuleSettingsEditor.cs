using PLUME.Core.Settings;
using UnityEditor;

namespace PLUME.Editor.Core.Settings
{
    [CustomEditor(typeof(FrameDataRecorderModuleSettings), true)]
    public class FrameDataRecorderModuleSettingsEditor : SettingsEditor<FrameDataRecorderModuleSettings>
    {
        protected const string ScriptPropertyPath = "m_Script";
        private const string EnabledPropertyPath = nameof(RecorderModuleSettings.enabled);
        private SerializedProperty _enabledProperty;

        private FrameRecorderModuleSettings _frameRecorderModuleSettings;
        private SerializedObject _serializedSettings;

        private void OnEnable()
        {
            _frameRecorderModuleSettings = GetSettings<FrameRecorderModuleSettings>();
            _serializedSettings = new SerializedObject(target);
            _enabledProperty = _serializedSettings.FindProperty(EnabledPropertyPath);
        }

        public override void OnInspectorGUI()
        {
            _serializedSettings.Update();

            if (!_frameRecorderModuleSettings.enabled)
                EditorGUILayout.HelpBox(
                    "This module is implicitly disabled because the 'Unity Frames' is disabled.",
                    MessageType.Info);

            EditorGUILayout.PropertyField(_enabledProperty);

            DrawPropertiesExcluding(_serializedSettings, ScriptPropertyPath, EnabledPropertyPath);

            _serializedSettings.ApplyModifiedProperties();
        }
    }
}