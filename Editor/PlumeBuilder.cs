using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

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
            };
            
            BuildPlayerWindow.RegisterBuildPlayerHandler(buildPlayerOptions =>
            {
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

            var scenesAssetsDependencies = GetFilteredScenesDependencies(scenePaths);

            if (GraphicsSettings.currentRenderPipeline != null)
            {
                var currentRenderPipelineAssetPath = AssetDatabase.GetAssetPath(GraphicsSettings.currentRenderPipeline);

                if (!scenesAssetsDependencies.Contains(currentRenderPipelineAssetPath))
                {
                    scenesAssetsDependencies.Add(currentRenderPipelineAssetPath);
                }
            }

            var assetBundleBuild = new AssetBundleBuild
            {
                assetBundleName = "plume_asset_bundle_windows",
                assetNames = scenesAssetsDependencies.ToArray()
            };

            if (!Directory.Exists(Application.streamingAssetsPath))
                Directory.CreateDirectory(Application.streamingAssetsPath);

            BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath, new[] { assetBundleBuild }, BuildAssetBundleOptions.StrictMode, BuildTarget.StandaloneWindows64);
            Debug.Log("Finished building asset bundle.");
        }

        private static List<string> GetFilteredScenesDependencies(string[] scenePaths)
        {
            var filteredDependencyPaths = new List<string>();

            var dependencies = AssetDatabase.GetDependencies(scenePaths, true);

            foreach (var dependencyPath in dependencies)
            {
                if (dependencyPath.StartsWith("Packages/com.unity.inputsystem/InputSystem/Editor/"))
                    continue;

                var assetType = AssetDatabase.GetMainAssetTypeAtPath(dependencyPath);
                var assetExtension = Path.GetExtension(dependencyPath);

                var excludedTypes = new[] { typeof(SceneAsset), typeof(MonoScript), typeof(AnimatorController) };
                var excludedExtensions = new[] { ".inputactions" };

                if (excludedTypes.Contains(assetType) || excludedExtensions.Contains(assetExtension))
                    continue;

                filteredDependencyPaths.Add(dependencyPath);
            }

            return filteredDependencyPaths;
        }
    }
}