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
                "The Unity frame recorder module records frame data from the Unity engine. " +
                "Frames are recorded in the late update of the recorder (i.e. at approximately the update rate " +
                "specified in the global recorder settings.)",
                MessageType.Info);

            DrawPropertiesExcluding(serializedObject, ScriptPropertyPath);
            serializedObject.ApplyModifiedProperties();
        }
    }
}