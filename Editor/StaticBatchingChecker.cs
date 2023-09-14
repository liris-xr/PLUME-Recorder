using System;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace PLUME.Editor
{
    public class StaticBatchingChecker : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            CheckStaticBatchingDisabled(report.summary.platform);
        }

        [InitializeOnLoadMethod]
        private static void OnPlayModeChanged()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode)
                    try
                    {
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                            CheckStaticBatchingDisabled(BuildTarget.StandaloneWindows64);
                        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                            CheckStaticBatchingDisabled(BuildTarget.StandaloneOSX);
                        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                            CheckStaticBatchingDisabled(BuildTarget.StandaloneLinux64);
                        else
                            Debug.LogWarning("Can't determine platform.");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                        EditorApplication.ExitPlaymode();
                    }
            };
        }

        private static SerializedObject GetSerializedPlayerSettings()
        {
            var methodInfo =
                typeof(PlayerSettings).GetMethod("GetSerializedObject", BindingFlags.NonPublic | BindingFlags.Static);
            return (SerializedObject)methodInfo?.Invoke(null, null);
        }

        private static void CheckStaticBatchingDisabled(BuildTarget target)
        {
            var playerSettings = GetSerializedPlayerSettings();

            var buildTargetsSize = playerSettings.FindProperty("m_BuildTargetBatching.Array.size").intValue;

            for (var buildTargetIdx = 0; buildTargetIdx < buildTargetsSize; ++buildTargetIdx)
            {
                var buildTarget = playerSettings
                    .FindProperty($"m_BuildTargetBatching.Array.data[{buildTargetIdx}].m_BuildTarget").stringValue;
                var staticBatchingEnabled = playerSettings
                    .FindProperty($"m_BuildTargetBatching.Array.data[{buildTargetIdx}].m_StaticBatching").boolValue;

                if ((staticBatchingEnabled &&
                     buildTarget == "Standalone" && target is BuildTarget.StandaloneLinux64
                         or BuildTarget.StandaloneWindows
                         or BuildTarget.StandaloneWindows64) ||
                    (buildTarget == "Android" && target is BuildTarget.Android) ||
                    (buildTarget == "iOS" && target is BuildTarget.iOS) ||
                    (buildTarget == "WebGL" && target is BuildTarget.WebGL))
                    throw new Exception(
                        "Static batching is not yet supported by PLUME. You can disable it in Edit > Project Settings... > Player > Settings for [YOUR_PLATFORM] > Other Settings > Static Batching.");
            }
        }
    }
}