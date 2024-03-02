using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
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

            var previousBuildTarget = EditorUserBuildSettings.activeBuildTarget;
            
            if (previousBuildTarget != target)
            {
                Logger.Log($"Please switch your platform to {target} to build the asset bundle.");
            }
            else
            {

                Directory.CreateDirectory(outputPath);
                CompatibilityBuildPipeline.BuildAssetBundles(outputPath, builds, options, target);

                File.Delete(zipOutputPath);
                ZipFile.CreateFromDirectory(outputPath, zipOutputPath, CompressionLevel.Optimal, false);
                Logger.Log($"Asset bundle built at {zipOutputPath}");
            }
        }

        private static bool CanAddAsset(ICollection<string> assetPaths, string assetPath)
        {
            // If already added, return false
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