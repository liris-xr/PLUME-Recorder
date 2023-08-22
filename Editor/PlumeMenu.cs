using System;
using UnityEditor;
using UnityEngine;

//https://manuel-rauber.com/2022/05/23/instantiate-your-own-prefabs-via-gameobject-menu/
namespace PLUME.Editor
{
    public static class PlumeMenu
    {
        private const int MenuPriority = -50;

        private const string PrefabManagerPath =
            "Packages/fr.liris.plume.recorder/ScriptableObjects/EditorExtensions/PlumePrefabManager.asset";

        private static PlumePrefabManager LocatePrefabManager()
        {
            return AssetDatabase.LoadAssetAtPath<PlumePrefabManager>(PrefabManagerPath);
        }

        private static void SafeInstantiate(Func<PlumePrefabManager, GameObject> itemSelector)
        {
            var prefabManager = LocatePrefabManager();

            if (!prefabManager)
            {
                Debug.LogWarning($"PrefabManager not found at path {PrefabManagerPath}");
                return;
            }

            var item = itemSelector(prefabManager);
            var instance = PrefabUtility.InstantiatePrefab(item, Selection.activeTransform);

            Undo.RegisterCreatedObjectUndo(instance, $"Create {instance.name}");
            Selection.activeObject = instance;
        }

        [MenuItem("GameObject/PLUME/Recorder", priority = MenuPriority)]
        private static void CreateRecorder()
        {
            SafeInstantiate(prefabManager => prefabManager.recorderPrefab);
        }
    }
}