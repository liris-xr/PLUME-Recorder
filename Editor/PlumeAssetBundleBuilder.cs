using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PLUME.Editor
{
    [InitializeOnLoad]
    public class PlumeAssetBundleBuilder
    {
        static PlumeAssetBundleBuilder()
        {
            BuildPlayerWindow.RegisterBuildPlayerHandler(buildPlayerOptions =>
            {
                BuildAssetBundle();
                BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(buildPlayerOptions);
            });
        }

        [MenuItem("PLUME/Build Asset Bundle")]
        private static void BuildAssetBundle()
        {
            var scenePaths = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .DefaultIfEmpty(SceneManager.GetActiveScene().path).ToArray();

            var scenesAssetsDependencies = GetFilteredScenesDependencies(scenePaths);

            var assetBundleBuild = new AssetBundleBuild
            {
                assetBundleName = "plume_asset_bundle_windows",
                assetNames = scenesAssetsDependencies
            };

            if (!Directory.Exists(Application.streamingAssetsPath))
                Directory.CreateDirectory(Application.streamingAssetsPath);

            BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath, new[] { assetBundleBuild }, BuildAssetBundleOptions.StrictMode, BuildTarget.StandaloneWindows);
            Debug.Log("Finished building asset bundle.");
        }

        private static string[] GetFilteredScenesDependencies(string[] scenePaths)
        {
            var filteredDependencyPaths = new List<string>(scenePaths.Length);

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

            return filteredDependencyPaths.ToArray();
        }
    }
}