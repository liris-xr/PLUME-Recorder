using PLUME.Core.Settings;
using UnityEditor;

namespace PLUME.Editor.Core.Settings
{
    [CustomEditor(typeof(FrameDataRecorderModuleSettings), true)]
    public class FrameDataRecorderModuleSettingsEditor : SettingsEditor<FrameDataRecorderModuleSettings>
    {
        protected const string ScriptPropertyPath = "m_Script";
        private SerializedObject _serializedSettings;

        private void OnEnable()
        {
            _serializedSettings = new SerializedObject(target);
        }

        public override void OnInspectorGUI()
        {
            _serializedSettings.Update();
            DrawPropertiesExcluding(_serializedSettings, ScriptPropertyPath);
            _serializedSettings.ApplyModifiedProperties();
        }
    }
}