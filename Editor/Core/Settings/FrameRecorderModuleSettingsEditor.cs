using PLUME.Core.Settings;
using UnityEditor;

namespace PLUME.Editor.Core.Settings
{
    [CustomEditor(typeof(FrameRecorderModuleSettings))]
    public class FrameRecorderModuleSettingsEditor : UnityEditor.Editor
    {
        private const string ScriptPropertyPath = "m_Script";

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, ScriptPropertyPath);
            serializedObject.ApplyModifiedProperties();
        }
    }
}