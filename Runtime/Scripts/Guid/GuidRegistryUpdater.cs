#if UNITY_EDITOR

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
#endif

namespace PLUME.Guid
{
    [InitializeOnLoad]
    public class GuidRegistryUpdater : IPreprocessBuildWithReport
    {
        static GuidRegistryUpdater()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state != PlayModeStateChange.ExitingEditMode) return;
                UpdateGuidRegistries(new[] { SceneManager.GetActiveScene().path });
            };
        }

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
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

            var assetsGuidRegistry = AssetsGuidRegistry.Instance;

            var prevAssetsGuid = assetsGuidRegistry.Copy();

            assetsGuidRegistry.Clear();
            AssetsGuidRegistry.CommitRegistry(assetsGuidRegistry);

            foreach (var scenePath in scenePaths)
            {
                var scene = EditorSceneManager.OpenScene(scenePath);
                var objects = new List<Object>();

                var sceneObjectsGuidRegistry = SceneObjectsGuidRegistry.GetOrCreateInScene(scene);
                var prevSceneObjectsGuid = sceneObjectsGuidRegistry.Copy();

                sceneObjectsGuidRegistry.Clear();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);

                objects.AddRange(scene.GetRootGameObjects());
                objects.AddRange(EditorUtility.CollectDependencies(scene.GetRootGameObjects()));

                // Lighting settings
                var lightingSettings = Lightmapping.GetLightingSettingsForScene(scene);
                if (lightingSettings != null)
                {
                    objects.Add(lightingSettings);
                    objects.AddRange(EditorUtility.CollectDependencies(new[] { lightingSettings }));
                }

                var lightingDataAsset = Lightmapping.lightingDataAsset;
                if (lightingDataAsset != null)
                {
                    objects.Add(lightingDataAsset);
                    objects.AddRange(EditorUtility.CollectDependencies(new[] { lightingDataAsset }));
                }

                // Render Settings dependencies
                objects.Add(RenderSettings.skybox);
                objects.Add(RenderSettings.sun);

#if UNITY_2022_1_OR_NEWER
                objects.Add(RenderSettings.customReflectionTexture);
#else
                objects.Add(RenderSettings.customReflection);
#endif
                
                objects.Add(GraphicsSettings.currentRenderPipeline);

                // Add any missing dependencies
                // This can happen for properties that are not visible by the EditorUtility.CollectDependencies,
                // like reflection probes cubemaps.
                var dependencies = AssetDatabase.GetDependencies(scene.path, true);
                foreach (var dependency in dependencies)
                {
                    var asset = AssetDatabase.LoadAssetAtPath<Object>(dependency);
                    if (objects.Contains(asset)) continue;
                    if (asset is SceneAsset) continue;
                    objects.Add(asset);
                }

                foreach (var obj in objects)
                {
                    if (obj == null)
                        continue;

                    var isAsset = AssetDatabase.Contains(obj);

                    if (isAsset)
                    {
                        assetsGuidRegistry.TryAdd(prevAssetsGuid.TryGetValue(obj, out var prevAssetGuid)
                            ? prevAssetGuid
                            : AssetsGuidRegistry.CreateNewEntry(obj));
                    }
                    else
                    {
                        // Do not add Recorder and its children
                        if (obj is GameObject go && (go.GetComponent<Recorder>() != null ||
                                                     go.GetComponentInParent<Recorder>() != null))
                        {
                            continue;
                        }

                        if (obj is Component comp && (comp.gameObject.GetComponent<Recorder>() != null ||
                                                      comp.GetComponentInParent<Recorder>() != null))
                        {
                            continue;
                        }

                        sceneObjectsGuidRegistry.TryAdd(
                            prevSceneObjectsGuid.TryGetValue(obj, out var prevSceneObjectGuid)
                                ? prevSceneObjectGuid
                                : SceneObjectsGuidRegistry.CreateNewEntry(obj));
                    }
                }

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);

                AssetsGuidRegistry.CommitRegistry(assetsGuidRegistry);
            }

            EditorSceneManager.OpenScene(activeScenePath);
        }
    }
}
#endif