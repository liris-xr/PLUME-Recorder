using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build.Pipeline;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityRuntimeGuid.Editor;
using CompressionLevel = System.IO.Compression.CompressionLevel;
using Logger = PLUME.Core.Logger;

namespace PLUME.Editor
{
    [InitializeOnLoad]
    public class AssetBundleBuilder
    {
        static AssetBundleBuilder()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state != PlayModeStateChange.ExitingEditMode) return;

                GetBatchingForPlatform(EditorUserBuildSettings.activeBuildTarget, out var staticBatching,
                    out var dynamicBatching);

                if (staticBatching != 0 || dynamicBatching != 0)
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

                GetBatchingForPlatform(buildPlayerOptions.target, out var staticBatching,
                    out var dynamicBatching);

                if (staticBatching != 0 || dynamicBatching != 0)
                    SetBatchingForPlatform(buildPlayerOptions.target, 0, 0);
                
                BuildAssetBundle();
                BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(buildPlayerOptions);
            });
        }


        public static void BuildAssetBundle()
        {
            var scenePaths = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .DefaultIfEmpty(SceneManager.GetActiveScene().path).ToArray();

            var assetsPaths = new List<string>();

            foreach (var scenePath in scenePaths)
            {
                AddAssetWithDependencies(assetsPaths, scenePath);
            }

            if (GraphicsSettings.defaultRenderPipeline != null)
            {
                var defaultRenderPipelineAssetPath = AssetDatabase.GetAssetPath(GraphicsSettings.defaultRenderPipeline);
                AddAssetWithDependencies(assetsPaths, defaultRenderPipelineAssetPath);
            }

            for (var qualityLevel = 0; qualityLevel < QualitySettings.names.Length; qualityLevel++)
            {
                var qualityLevelRenderPipeline = QualitySettings.GetRenderPipelineAssetAt(qualityLevel);
                if (qualityLevelRenderPipeline == null) continue;
                var qualityLevelRenderPipelineAssetPath = AssetDatabase.GetAssetPath(qualityLevelRenderPipeline);
                AddAssetWithDependencies(assetsPaths, qualityLevelRenderPipelineAssetPath);
            }

            var assetsBuild = new AssetBundleBuild
            {
                assetBundleName = "plume_assets",
                assetNames = assetsPaths.ToArray()
            };

            var scenesBuild = new AssetBundleBuild
            {
                assetBundleName = "plume_scenes",
                assetNames = scenePaths.ToArray()
            };

            var outputPath = Path.Join(Application.dataPath, "AssetBundles/plume_bundle/");
            var zipOutputPath = Path.Join(Application.streamingAssetsPath, "plume_bundle.zip");
            var builds = new[] { assetsBuild, scenesBuild };
            const BuildAssetBundleOptions options = BuildAssetBundleOptions.ChunkBasedCompression;
            const BuildTarget target = BuildTarget.StandaloneWindows64;

            Directory.CreateDirectory(outputPath);
            CompatibilityBuildPipeline.BuildAssetBundles(outputPath, builds, options, target);

            File.Delete(zipOutputPath);
            ZipFile.CreateFromDirectory(outputPath, zipOutputPath, CompressionLevel.Optimal, false);
        }

        private static void SetBatchingForPlatform(BuildTarget platform, int staticBatching, int dynamicBatching)
        {
            Logger.Log("Disabling static and dynamic batching for PLUME.");

            var setter = typeof(PlayerSettings).GetMethod("SetBatchingForPlatform",
                BindingFlags.Static | BindingFlags.Default | BindingFlags.NonPublic);

            if (setter == null)
            {
                throw new NotSupportedException("Setting batching per platform is not supported");
            }

            var args = new object[]
            {
                platform,
                staticBatching,
                dynamicBatching
            };

            setter.Invoke(null, args);
        }

        private static void GetBatchingForPlatform(BuildTarget platform, out int staticBatching,
            out int dynamicBatching)
        {
            var getter = typeof(PlayerSettings).GetMethod("GetBatchingForPlatform",
                BindingFlags.Static | BindingFlags.Default | BindingFlags.NonPublic);

            if (getter == null)
            {
                throw new NotSupportedException("Getting batching per platform is not supported");
            }

            var args = new object[]
            {
                platform,
                null,
                null
            };

            getter.Invoke(null, args);

            staticBatching = (int)args[1];
            dynamicBatching = (int)args[2];
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