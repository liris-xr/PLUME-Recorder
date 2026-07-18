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
using PLUME.Core.Settings;
using PLUME.Editor.Core.Settings;
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

                    // TODO: if referencing a file nested in a prefab, add the full prefab
                }
            }

            // Bundling a render pipeline asset drags in its whole shader-variant set — the dominant
            // build cost. Only include the ones the user asked for (default: the active pipeline).
            var recorderSettings = SettingsEditor.GetSettings<RecorderSettings>();
            var exportedRenderPipelineAssets = GetRenderPipelineAssetsToExport(recorderSettings).ToList();
            foreach (var renderPipelineAsset in exportedRenderPipelineAssets)
            {
                var renderPipelineAssetPath = AssetDatabase.GetAssetPath(renderPipelineAsset);
                if (!string.IsNullOrEmpty(renderPipelineAssetPath))
                    assetsPaths.Add(renderPipelineAssetPath);
            }

            var exportedRenderPipelineNames = exportedRenderPipelineAssets.Count > 0
                ? string.Join(", ", exportedRenderPipelineAssets.Select(rp => rp.name))
                : "(none)";
            Logger.Log($"Building asset bundle. Render pipeline export mode: {recorderSettings.RenderPipelineExport}. " +
                       $"Exporting render pipeline assets: {exportedRenderPipelineNames}.");

#if URP_ENABLED
            var urpGlobalSettings = GraphicsSettings.GetSettingsForRenderPipeline<UnityEngine.Rendering.Universal.UniversalRenderPipeline>();
            if (urpGlobalSettings != null)
            {
                var urpGlobalSettingsPath = AssetDatabase.GetAssetPath(urpGlobalSettings);
                if (!string.IsNullOrEmpty(urpGlobalSettingsPath))
                    assetsPaths.Add(urpGlobalSettingsPath);
            }
#endif

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

            var outputPath = Path.Join(Application.dataPath, "AssetBundles", "plume_bundle/");
            var zipOutputPath = Path.Join(Application.dataPath, "AssetBundles", "plume_bundle.zip");
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
                // Bundle is already LZ4-compressed (ChunkBasedCompression); Deflate gains ~15%
                // but higher levels add <0.5% for ~2x the time. Fastest keeps the size, halves the zip.
                ZipFile.CreateFromDirectory(outputPath, zipOutputPath, CompressionLevel.Fastest, false);
                Logger.Log(
                    $"Asset bundle built at {zipOutputPath}.\n\nIncluded assets:\n{string.Join("\n", assetsPaths)}\n\nIncluded scenes:\n{string.Join("\n ", scenePaths)}");
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to build asset bundle.", e);
            }
        }

        private static IEnumerable<RenderPipelineAsset> GetRenderPipelineAssetsToExport(RecorderSettings settings)
        {
            switch (settings.RenderPipelineExport)
            {
                case RenderPipelineExportMode.CurrentActive:
                    if (GraphicsSettings.currentRenderPipeline != null)
                        yield return GraphicsSettings.currentRenderPipeline;
                    break;
                case RenderPipelineExportMode.All:
                    if (GraphicsSettings.defaultRenderPipeline != null)
                        yield return GraphicsSettings.defaultRenderPipeline;
                    for (var qualityLevel = 0; qualityLevel < QualitySettings.count; qualityLevel++)
                    {
                        var qualityLevelRenderPipeline = QualitySettings.GetRenderPipelineAssetAt(qualityLevel);
                        if (qualityLevelRenderPipeline != null)
                            yield return qualityLevelRenderPipeline;
                    }
                    break;
                case RenderPipelineExportMode.Custom:
                    // Skip null (deleted/unassigned) refs so a stale selection can't break the build.
                    foreach (var renderPipelineAsset in settings.CustomRenderPipelineAssets)
                        if (renderPipelineAsset != null)
                            yield return renderPipelineAsset;
                    break;
            }
        }
    }
}