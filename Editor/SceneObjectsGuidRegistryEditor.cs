using PLUME.Guid;
using UnityEditor;
using UnityEngine;

namespace PLUME.Editor
{
    [CustomEditor(typeof(SceneObjectsGuidRegistry))]
    public class SceneObjectsGuidRegistryEditor : UnityEditor.Editor
    {
        private SerializedProperty _registryEntries;

        private void OnEnable()
        {
            _registryEntries = serializedObject.FindProperty("registry").FindPropertyRelative("entries");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorStyles.label.wordWrap = true;

            _registryEntries.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(_registryEntries.isExpanded, "Scene objects");

            if (_registryEntries.isExpanded)
            {
                EditorGUI.indentLevel = 1;

                var sceneObjectIdx = 0;
                for (var i = 0; i < _registryEntries.arraySize; ++i)
                {
                    var entry = _registryEntries.GetArrayElementAtIndex(i);

                    var guid = entry.FindPropertyRelative("guid");
                    var @object = entry.FindPropertyRelative("object");
                    var objectType = @object.objectReferenceValue.GetType().Name;
                    var objectName = @object.objectReferenceValue.name;

                    entry.isExpanded =
                        EditorGUILayout.Foldout(entry.isExpanded,
                            $"Scene Object {sceneObjectIdx} ({objectName} | {objectType})", true);

                    if (entry.isExpanded)
                    {
                        EditorGUI.indentLevel = 2;
                        GUI.enabled = false;
                        EditorGUILayout.ObjectField("Scene Object", @object.objectReferenceValue, typeof(Object), false);
                        EditorGUILayout.TextField("GUID", guid.stringValue);
                        GUI.enabled = true;
                        EditorGUI.indentLevel = 1;
                    }

                    ++sceneObjectIdx;
                }

                EditorGUI.indentLevel = 0;
            }

            EditorGUI.EndFoldoutHeaderGroup();
            serializedObject.ApplyModifiedProperties();
        }
    }
}