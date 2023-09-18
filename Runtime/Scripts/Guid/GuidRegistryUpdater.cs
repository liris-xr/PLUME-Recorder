#if UNITY_EDITOR
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PLUME.Guid
{
    [InitializeOnLoad]
    public static class GuidRegistryEditorUpdater
    {
        static GuidRegistryEditorUpdater()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state != PlayModeStateChange.ExitingEditMode) return;
                GuidRegistryUpdater.UpdateGuidRegistries(new[] { SceneManager.GetActiveScene().path });
            };
        }
    }

    public class GuidRegistryUpdater : IPreprocessBuildWithReport
    {
        public int callbackOrder => int.MaxValue;

        public void OnPreprocessBuild(BuildReport report)
        {
            var scenePaths = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .DefaultIfEmpty(SceneManager.GetActiveScene().path);

            UpdateGuidRegistries(scenePaths);
        }

        [SuppressMessage("ReSharper", "CoVariantArrayConversion")]
        public static void UpdateGuidRegistries(IEnumerable<string> scenePaths)
        {
            var activeScenePath = SceneManager.GetActiveScene().path;

            var assetsGuidRegistry = AssetsGuidRegistry.Get();
            
            var prevAssetsGuid = assetsGuidRegistry.Copy();

            assetsGuidRegistry.Clear();
            EditorUtility.SetDirty(assetsGuidRegistry);
            AssetDatabase.SaveAssetIfDirty(assetsGuidRegistry);

            foreach (var scenePath in scenePaths)
            {
                var scene = EditorSceneManager.OpenScene(scenePath);
                var objects = new List<Object>();

                // // EditorSceneManager.OpenScene might unload the AssetGuidRegistry. We ensure that we keep it loaded.
                // assetsGuidRegistry = AssetDatabase.LoadAssetAtPath<AssetsGuidRegistry>(AssetsGuidRegistry.AssetPath);

                var sceneObjectsGuidRegistry = SceneObjectsGuidRegistry.GetOrCreateInScene(scene);
                var prevSceneObjectsGuid = sceneObjectsGuidRegistry.Copy();

                sceneObjectsGuidRegistry.Clear();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);

                objects.AddRange(scene.GetRootGameObjects());
                objects.AddRange(EditorUtility.CollectDependencies(scene.GetRootGameObjects()));

                foreach (var obj in objects)
                {
                    var isAsset = AssetDatabase.Contains(obj);

                    if (isAsset)
                    {
                        assetsGuidRegistry.TryAdd(prevAssetsGuid.TryGetValue(obj, out var prevAssetGuid)
                            ? prevAssetGuid
                            : AssetsGuidRegistry.CreateNewEntry(obj));
                    }
                    else
                    {
                        sceneObjectsGuidRegistry.TryAdd(
                            prevSceneObjectsGuid.TryGetValue(obj, out var prevSceneObjectGuid)
                                ? prevSceneObjectGuid
                                : SceneObjectsGuidRegistry.CreateNewEntry(obj));
                    }
                }

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);

                EditorUtility.SetDirty(assetsGuidRegistry);
                AssetDatabase.SaveAssetIfDirty(assetsGuidRegistry);
            }

            EditorSceneManager.OpenScene(activeScenePath);
        }
    }
}
#endif