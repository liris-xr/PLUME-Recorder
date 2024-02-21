using UnityEditor;
using UnityEngine;

namespace PLUME.Editor
{
    public class RecorderSettingsWindow : EditorWindow
    {
        [MenuItem("PLUME/Recorder Settings")]
        public static void ShowWindow()
        {
            GetWindow(typeof(RecorderSettingsWindow), false, "Recorder Settings");
        }
        
        public void OnGUI()
        {
            GUILayout.Label("Recorder Options", EditorStyles.boldLabel);
            GUILayout.Label("This is a window for setting up the Recorder options.");
        }
    }
}