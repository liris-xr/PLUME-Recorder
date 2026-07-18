using System.Collections.Generic;
using PLUME.Core.Settings;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace PLUME.Editor.Core.Settings
{
    [CustomEditor(typeof(RecorderSettings))]
    public class RecorderSettingsEditor : UnityEditor.Editor
    {
        protected const string ScriptPropertyPath = "m_Script";
        private const string ExportModePath = "renderPipelineExport";
        private const string CustomAssetsPath = "customRenderPipelineAssets";

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Global Settings", EditorStyles.boldLabel);
            // The render pipeline export fields get their own custom section below.
            DrawPropertiesExcluding(serializedObject, ScriptPropertyPath, ExportModePath, CustomAssetsPath);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Asset Bundle", EditorStyles.boldLabel);

            var modeProp = serializedObject.FindProperty(ExportModePath);
            EditorGUILayout.PropertyField(modeProp, new GUIContent("Render Pipeline Export"));

            if ((RenderPipelineExportMode)modeProp.enumValueIndex == RenderPipelineExportMode.Custom)
                DrawCustomRenderPipelineSelection();

            serializedObject.ApplyModifiedProperties();
        }

        // Presents live quality-level checkboxes but stores the underlying render pipeline asset
        // reference, so the selection survives quality-level rename/reorder. Assets no longer
        // mapped to any level are surfaced separately so nothing persists invisibly.
        private void DrawCustomRenderPipelineSelection()
        {
            var listProp = serializedObject.FindProperty(CustomAssetsPath);

            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField("Quality levels", EditorStyles.miniBoldLabel);
            var levelAssets = new HashSet<RenderPipelineAsset>();
            var names = QualitySettings.names;
            for (var i = 0; i < names.Length; i++)
            {
                var rp = QualitySettings.GetRenderPipelineAssetAt(i);
                using (new EditorGUI.DisabledScope(rp == null))
                {
                    var label = rp != null ? $"{names[i]}  ({rp.name})" : $"{names[i]}  (no render pipeline asset)";
                    var selected = rp != null && ListContains(listProp, rp);
                    var newSelected = EditorGUILayout.ToggleLeft(label, selected);
                    if (rp == null)
                        continue;
                    levelAssets.Add(rp);
                    if (newSelected && !selected) ListAdd(listProp, rp);
                    else if (!newSelected && selected) ListRemove(listProp, rp);
                }
            }

            var orphanIndices = new List<int>();
            for (var i = 0; i < listProp.arraySize; i++)
            {
                var obj = listProp.GetArrayElementAtIndex(i).objectReferenceValue as RenderPipelineAsset;
                if (obj == null || !levelAssets.Contains(obj))
                    orphanIndices.Add(i);
            }

            if (orphanIndices.Count > 0)
            {
                EditorGUILayout.Space(2);
                EditorGUILayout.LabelField("Also included (not tied to a quality level)", EditorStyles.miniBoldLabel);
                // Descending so a removal does not shift the indices we still have to visit.
                for (var k = orphanIndices.Count - 1; k >= 0; k--)
                {
                    var idx = orphanIndices[k];
                    var elem = listProp.GetArrayElementAtIndex(idx);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        using (new EditorGUI.DisabledScope(true))
                            EditorGUILayout.ObjectField(elem.objectReferenceValue, typeof(RenderPipelineAsset), false);
                        if (GUILayout.Button("Remove", GUILayout.Width(70)))
                            DeleteAt(listProp, idx);
                    }
                }
            }

            EditorGUI.indentLevel--;
        }

        private static bool ListContains(SerializedProperty listProp, RenderPipelineAsset asset)
        {
            for (var i = 0; i < listProp.arraySize; i++)
                if (listProp.GetArrayElementAtIndex(i).objectReferenceValue == asset)
                    return true;
            return false;
        }

        private static void ListAdd(SerializedProperty listProp, RenderPipelineAsset asset)
        {
            var idx = listProp.arraySize;
            listProp.InsertArrayElementAtIndex(idx);
            listProp.GetArrayElementAtIndex(idx).objectReferenceValue = asset;
        }

        private static void ListRemove(SerializedProperty listProp, RenderPipelineAsset asset)
        {
            for (var i = 0; i < listProp.arraySize; i++)
            {
                if (listProp.GetArrayElementAtIndex(i).objectReferenceValue == asset)
                {
                    DeleteAt(listProp, i);
                    return;
                }
            }
        }

        // For object-reference arrays, DeleteArrayElementAtIndex first nulls a non-null element
        // rather than removing it; null it explicitly so a single call removes the slot.
        private static void DeleteAt(SerializedProperty listProp, int index)
        {
            listProp.GetArrayElementAtIndex(index).objectReferenceValue = null;
            listProp.DeleteArrayElementAtIndex(index);
        }
    }
}
