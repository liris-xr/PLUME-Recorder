using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityRuntimeGuid.Editor;

namespace PLUME.Editor
{
    [InitializeOnLoad]
    public class PlumeBuilder
    {
        static PlumeBuilder()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state != PlayModeStateChange.ExitingEditMode) return;
                
                SetBatchingForPlatform(EditorUserBuildSettings.activeBuildTarget, 0, 0);
                
                var activeScene = SceneManager.GetActiveScene();
                EditorSceneManager.MarkSceneDirty(activeScene);
                EditorSceneManager.SaveScene(activeScene);

                // Update the registries when entering play mode in the Editor
                GuidRegistryUpdater.UpdateAssetsGuidRegistry(GuidRegistryUpdater.GetAllScenePaths(true));
                GuidRegistryUpdater.UpdateScenesGuidRegistry(GuidRegistryUpdater.GetAllScenePaths(true));
            };
            
            BuildPlayerWindow.RegisterBuildPlayerHandler(buildPlayerOptions =>
            {
                // Update the registries when building the application
                GuidRegistryUpdater.UpdateAssetsGuidRegistry(GuidRegistryUpdater.GetAllScenePaths(false));
                GuidRegistryUpdater.UpdateScenesGuidRegistry(GuidRegistryUpdater.GetAllScenePaths(false));
                
                SetBatchingForPlatform(buildPlayerOptions.target, 0, 0);
                BuildAssetBundle();
                BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(buildPlayerOptions);
            });
        }

        private static void SetBatchingForPlatform(BuildTarget platform, int staticBatching, int dynamicBatching)
        {
            Debug.Log("Disabling static and dynamic batching for PLUME.");
            var method = typeof(PlayerSettings).GetMethod("SetBatchingForPlatform", BindingFlags.Static | BindingFlags.Default | BindingFlags.NonPublic);
   
            if (method == null)
            {
                throw new NotSupportedException("Setting batching per platform is not supported");
            }
 
            var args = new object[]
            {
                platform,
                staticBatching,
                dynamicBatching
            };
 
            method.Invoke(null, args);
        }

        [MenuItem("PLUME/Build Asset Bundle")]
        private static void BuildAssetBundle()
        {
            var scenePaths = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .DefaultIfEmpty(SceneManager.GetActiveScene().path).ToArray();

            var assetPaths = new List<string>();

            foreach (var scenePath in scenePaths)
            {
                AddAssetWithDependencies(assetPaths, scenePath);
            }
            
            if (GraphicsSettings.defaultRenderPipeline != null)
            {
                var defaultRenderPipelineAssetPath = AssetDatabase.GetAssetPath(GraphicsSettings.defaultRenderPipeline);
                AddAssetWithDependencies(assetPaths, defaultRenderPipelineAssetPath);
            }

            for (var qualityLevel = 0; qualityLevel < QualitySettings.names.Length; qualityLevel++)
            {
                var qualityLevelRenderPipeline = QualitySettings.GetRenderPipelineAssetAt(qualityLevel);
                if (qualityLevelRenderPipeline == null) continue;
                var qualityLevelRenderPipelineAssetPath = AssetDatabase.GetAssetPath(qualityLevelRenderPipeline);
                AddAssetWithDependencies(assetPaths, qualityLevelRenderPipelineAssetPath);
            }

            var assetBundleBuild = new AssetBundleBuild
            {
                assetBundleName = "plume_asset_bundle_windows",
                assetNames = assetPaths.ToArray()
            };

            if (!Directory.Exists(Application.streamingAssetsPath))
                Directory.CreateDirectory(Application.streamingAssetsPath);

            BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath, new[] { assetBundleBuild }, BuildAssetBundleOptions.StrictMode, BuildTarget.StandaloneWindows64);
            Debug.Log("Finished building asset bundle.");
        }

        private static bool CanAddAsset(ICollection<string> assetPaths, string assetPath)
        {
            if (assetPaths.Contains(assetPath))
                return false;

            var assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);

            if (assetType == null)
                return false;
            
            return assetType.Namespace == null || !assetType.Namespace.StartsWith("UnityEditor");
        }
        
        private static void AddAssetWithDependencies(ICollection<string> assetPaths, string assetPath)
        {
            if (CanAddAsset(assetPaths, assetPath))
            {
                assetPaths.Add(assetPath);
            }
            
            var dependenciesPaths = AssetDatabase.GetDependencies(assetPath, true);

            foreach (var dependencyPath in dependenciesPaths)
            {
                if (CanAddAsset(assetPaths, dependencyPath))
                {
                    assetPaths.Add(dependencyPath);
                }
            }
        }
    }
}