using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEditor.Build.Pipeline;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using CompressionLevel = System.IO.Compression.CompressionLevel;
using Logger = PLUME.Core.Logger;

namespace PLUME.Editor
{
    public static class AssetBundleBuilder
    {
        public static void BuildAssetBundle()
        {
            var scenePaths = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .DefaultIfEmpty(SceneManager.GetActiveScene().path).ToArray();

            var buildSettings = new BuildSettings
            {
                target = BuildTarget.StandaloneWindows64,
                buildFlags = ContentBuildFlags.None,
                group = BuildTargetGroup.Standalone,
                subtarget = (int)StandaloneBuildSubtarget.Player
            };
            var buildUsageTagSet = new BuildUsageTagSet();
            var usageCache = new BuildUsageCache();

            var assetsPaths = new HashSet<string>();

            foreach (var scenePath in scenePaths)
            {
                var sceneDependencyInfo = ContentBuildInterface.CalculatePlayerDependenciesForScene(scenePath,
                    buildSettings, buildUsageTagSet, usageCache, DependencyType.DefaultDependencies);

                foreach (var obj in sceneDependencyInfo.referencedObjects)
                {
                    if (obj.fileType is FileType.NonAssetType)
                        continue;
                    var assetPath = AssetDatabase.GUIDToAssetPath(obj.guid);
                    if (!string.IsNullOrEmpty(assetPath))
                        assetsPaths.Add(assetPath);
                }
            }

            if (GraphicsSettings.defaultRenderPipeline != null)
            {
                var defaultRenderPipelineAssetPath = AssetDatabase.GetAssetPath(GraphicsSettings.defaultRenderPipeline);

                if (!string.IsNullOrEmpty(defaultRenderPipelineAssetPath))
                    assetsPaths.Add(defaultRenderPipelineAssetPath);
            }

            for (var qualityLevel = 0; qualityLevel < QualitySettings.names.Length; qualityLevel++)
            {
                var qualityLevelRenderPipeline = QualitySettings.GetRenderPipelineAssetAt(qualityLevel);
                if (qualityLevelRenderPipeline == null) continue;
                var qualityLevelRenderPipelineAssetPath = AssetDatabase.GetAssetPath(qualityLevelRenderPipeline);
                if (!string.IsNullOrEmpty(qualityLevelRenderPipelineAssetPath))
                    assetsPaths.Add(qualityLevelRenderPipelineAssetPath);
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

            Directory.CreateDirectory(outputPath);

            // Switch platform to build
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone,
                BuildTarget.StandaloneWindows64);
            try
            {
                CompatibilityBuildPipeline.BuildAssetBundles(outputPath, builds, options,
                    BuildTarget.StandaloneWindows64);

                File.Delete(zipOutputPath);
                ZipFile.CreateFromDirectory(outputPath, zipOutputPath, CompressionLevel.Optimal, false);
                Logger.Log(
                    $"Asset bundle built at {zipOutputPath}.\n\nIncluded assets:\n{string.Join("\n", assetsPaths)}\n\nIncluded scenes:\n{string.Join("\n ", scenePaths)}");
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to build asset bundle.", e);
            }
        }
    }
}