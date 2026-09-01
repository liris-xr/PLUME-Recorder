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
                    if (string.IsNullOrEmpty(assetPath))
                        continue;

                    // Never bundle MonoScript assets: the replay side binds scripted objects by
                    // assembly/class name using its own compiled assemblies. Explicitly bundling a
                    // script redirects m_Script of ScriptableObjects in the same bundle to the bundled
                    // MonoScript, which fails to resolve at load time and silently drops the object
                    // (observed with VolumeProfile when VolumeProfile.cs was bundled).
                    if (AssetDatabase.GetMainAssetTypeAtPath(assetPath) == typeof(MonoScript))
                        continue;

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

            // HDRP references diffusion profiles by a baked GUID/hash rather than an object reference, so the scene
            // dependency walk above never includes them. Without the profile assets in the bundle, subsurface
            // materials (e.g. skin) fall back to the neutral profile and render bright red on replay. Add every
            // project diffusion profile explicitly. Queried by type name so no HDRP assembly reference is needed;
            // a no-op for non-HDRP projects (no such assets exist).
            var diffusionProfilePaths = new List<string>();
            foreach (var diffusionProfileGuid in AssetDatabase.FindAssets("t:DiffusionProfileSettings", new[] { "Assets" }))
            {
                var diffusionProfilePath = AssetDatabase.GUIDToAssetPath(diffusionProfileGuid);
                if (string.IsNullOrEmpty(diffusionProfilePath))
                    continue;
                assetsPaths.Add(diffusionProfilePath);
                diffusionProfilePaths.Add(diffusionProfilePath);
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

            // Output outside Assets/ so Unity doesn't re-import the built bundles as project assets.
            var assetBundlesDir = Path.GetFullPath(Path.Join(Application.dataPath, "..", "AssetBundles"));
            var outputPath = Path.Join(assetBundlesDir, "plume_bundle/");
            var zipOutputPath = Path.Join(assetBundlesDir, "plume_bundle.zip");
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

                WriteDiffusionProfileHashManifest(outputPath, diffusionProfilePaths);

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

        [Serializable]
        private class DiffusionProfileHashManifest
        {
            public List<DiffusionProfileHashEntry> entries = new();
        }

        [Serializable]
        private class DiffusionProfileHashEntry
        {
            public string name;
            public uint hash;
        }

        /// <summary>
        /// Writes a name-to-hash manifest for every bundled diffusion profile next to the bundles (zipped with
        /// them). HDRP derives a profile's hash from its asset GUID in the editor and bakes that value into
        /// materials, but when the viewer loads the profile from the bundle in the editor, HDRP re-derives the
        /// hash from the (nonexistent) AssetDatabase path and zeroes it, breaking the material-to-profile match
        /// (skin renders red). The viewer restores the hashes from this manifest after loading.
        /// The in-memory hash is read here (not the serialized one), which is always the GUID-derived value the
        /// materials were baked with, even if the profile asset was never saved after its hash was computed.
        /// </summary>
        private static void WriteDiffusionProfileHashManifest(string outputPath, List<string> diffusionProfilePaths)
        {
            var settingsType = Type.GetType(
                "UnityEngine.Rendering.HighDefinition.DiffusionProfileSettings, Unity.RenderPipelines.HighDefinition.Runtime");
            if (settingsType == null || diffusionProfilePaths.Count == 0)
                return;

            var profileField = settingsType.GetField("profile",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);

            var manifest = new DiffusionProfileHashManifest();

            foreach (var path in diffusionProfilePaths)
            {
                var settings = AssetDatabase.LoadAssetAtPath(path, settingsType);
                if (settings == null) continue;

                var diffusionProfile = profileField?.GetValue(settings);
                var hashField = diffusionProfile?.GetType().GetField("hash",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic);
                if (hashField?.GetValue(diffusionProfile) is not uint hash)
                    continue;

                if (hash == 0)
                {
                    Logger.LogWarning($"Diffusion profile '{settings.name}' ({path}) has hash 0 at build time; " +
                                      "subsurface materials using it will not match it on replay.");
                    continue;
                }

                manifest.entries.Add(new DiffusionProfileHashEntry { name = settings.name, hash = hash });
            }

            var manifestPath = Path.Join(outputPath, "plume_diffusion_hashes.json");
            File.WriteAllText(manifestPath, JsonUtility.ToJson(manifest, true));
            Logger.Log($"Wrote diffusion profile hash manifest with {manifest.entries.Count} entries to {manifestPath}.");
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