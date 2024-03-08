using PLUME.Core.Settings;
using UnityEditor;

namespace PLUME.Editor.Core.Settings
{
    [CustomEditor(typeof(RecorderSettings))]
    public class RecorderSettingsEditor : UnityEditor.Editor
    {
        protected const string ScriptPropertyPath = "m_Script";

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.LabelField("Global Settings", EditorStyles.boldLabel);
            DrawPropertiesExcluding(serializedObject, ScriptPropertyPath);
            serializedObject.ApplyModifiedProperties();
        }
    }
}