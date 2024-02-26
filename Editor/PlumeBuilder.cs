using System;
using System.Reflection;
using PLUME.Core.Settings;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityRuntimeGuid.Editor;
using Logger = PLUME.Core.Logger;

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
                Plume.IsBuilding = true;

                try
                {
                    foreach (var settings in Resources.LoadAll<Settings>(Settings.BasePath))
                    {
                        settings.OnValidate();
                    }

                    // Update the registries when building the application
                    GuidRegistryUpdater.UpdateAssetsGuidRegistry(GuidRegistryUpdater.GetAllScenePaths(false));
                    GuidRegistryUpdater.UpdateScenesGuidRegistry(GuidRegistryUpdater.GetAllScenePaths(false));

                    GetBatchingForPlatform(buildPlayerOptions.target, out var staticBatching,
                        out var dynamicBatching);

                    if (staticBatching != 0 || dynamicBatching != 0)
                        SetBatchingForPlatform(buildPlayerOptions.target, 0, 0);

                    if (buildPlayerOptions.target == BuildTarget.Android)
                    {
                        // Set the minimum SDK version to 24 (Android 7.0) for LSL support
                        if (PlayerSettings.Android.minSdkVersion < AndroidSdkVersions.AndroidApiLevel24)
                        {
                            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
                            Logger.LogWarning("Minimum SDK version set to 24 (Android 7.0) for LSL support.");
                        }
                    }

                    AssetBundleBuilder.BuildAssetBundle();
                    BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(buildPlayerOptions);
                }
                finally
                {
                    Plume.IsBuilding = false;
                }
            });
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
    }
}