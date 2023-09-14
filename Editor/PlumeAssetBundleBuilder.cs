using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.SceneManagement;
using Directory = UnityEngine.Windows.Directory;

namespace PLUME.Editor
{
    public static class PlumeAssetBundleBuilder
    {
        [MenuItem("PLUME/Build asset bundle")]
        public static void BuildAssetBundle()
        {
            var scenes = GetScenesIncludedInBuild();
            var scenePaths = scenes.Select(scene => scene.path).ToArray();
            var scenesAssetsDependencies = GetFilteredScenesDependencies(scenePaths);
            
            var assetBundleBuild = new AssetBundleBuild
            {
                assetBundleName = "plume_asset_bundle_windows",
                assetNames = scenesAssetsDependencies
            };

            // TODO: add warning if a hash collision is detected. Maybe propose a custom solution for this, like a custom correspondence table?
            // TODO: in the Editor, auto build the asset bundle before entering play mode

            if (!Directory.Exists(Application.streamingAssetsPath))
                Directory.CreateDirectory(Application.streamingAssetsPath);

            BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath, new[] {assetBundleBuild},
                BuildAssetBundleOptions.ForceRebuildAssetBundle, BuildTarget.StandaloneWindows);
            
            Debug.Log("Finished building asset bundle.");
        }

        private static string[] GetFilteredScenesDependencies(string[] scenePaths)
        {
            var filteredDependencyPaths = new List<string>(scenePaths.Length);

            var dependencies = AssetDatabase.GetDependencies(scenePaths);

            foreach (var dependencyPath in dependencies)
            {
                if(dependencyPath.StartsWith("Packages/com.unity.inputsystem/InputSystem/Editor/"))
                    continue;
                
                var assetType = AssetDatabase.GetMainAssetTypeAtPath(dependencyPath);
                var assetExtension = Path.GetExtension(dependencyPath);
                
                var excludedTypes = new[] { typeof(SceneAsset), typeof(MonoScript), typeof(AnimatorController) };
                var excludedExtensions = new [] {".inputactions"};

                if (excludedTypes.Contains(assetType) || excludedExtensions.Contains(assetExtension))
                    continue;

                filteredDependencyPaths.Add(dependencyPath);
            }

            return filteredDependencyPaths.ToArray();
        }

        private static IEnumerable<Scene> GetScenesIncludedInBuild()
        {
            var scenesInBuildSettings = EditorBuildSettings.scenes;
            var enabledScenesCountInBuildSettings = scenesInBuildSettings.Count(scene => scene.enabled);

            var scenes = new List<Scene>();

            if (enabledScenesCountInBuildSettings == 0)
                scenes.Add(SceneManager.GetActiveScene());
            else
                for (var sceneIdx = 0; sceneIdx < enabledScenesCountInBuildSettings; sceneIdx++)
                    scenes.Add(SceneManager.GetSceneByBuildIndex(sceneIdx));

            return scenes;
        }
    }
}