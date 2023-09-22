using System;
using UnityEditor;
using UnityEngine;

namespace PLUME.Editor
{
    [CustomEditor(typeof(Recorder))]
    public class RecorderCustomEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var recorder = (Recorder) target;

            if (recorder.IsRecording)
            {
                if (GUILayout.Button("Stop recording")) recorder.StopRecording();
            }
            else
            {
                if (GUILayout.Button("Start recording"))
                {
                    recorder.recordIdentifier = System.Guid.NewGuid().ToString();
                    recorder.StartRecording();
                }
            }
        }
    }
}