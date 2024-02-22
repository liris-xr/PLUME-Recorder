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

            EditorGUILayout.HelpBox(
                "The Unity frame recorder module records frame data from the Unity engine." +
                "It uses the update rate from the global recorder settings to determine when to record a new frame.",
                MessageType.Info);

            DrawPropertiesExcluding(serializedObject, ScriptPropertyPath);
            serializedObject.ApplyModifiedProperties();
        }
    }
}