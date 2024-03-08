using System.IO;
using PLUME.Core.Settings;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace PLUME.Editor
{
    public static class MenuItems
    {
        [MenuItem("PLUME/Build Asset Bundle", priority = 0)]
        private static void BuildAssetBundle()
        {
            AssetBundleBuilder.BuildAssetBundle();
        }
        
        [MenuItem("PLUME/Force Recompile With Hooks", priority = 1)]
        public static void ForceRecompileWithHooks()
        {
            if (Directory.Exists("Library/ScriptAssemblies"))
            {
                Directory.Delete("Library/ScriptAssemblies", true);
            }
            
            if (Directory.Exists("Library/Bee"))
            {
                Directory.Delete("Library/Bee", true);
            }
            
            CompilationPipeline.RequestScriptCompilation(RequestScriptCompilationOptions.CleanBuildCache);
        }
        
        [MenuItem("PLUME/Settings", priority = 10)]
        private static void OpenSettings()
        {
            SettingsService.OpenProjectSettings(RecorderSettings.SettingsWindowPath);
        }
        
        [MenuItem("PLUME/GitHub Repository", priority = 20)]
        private static void OpenGitHubRepository()
        {
            Application.OpenURL("https://www.github.com/liris-xr/PLUME");
        }
        
        [MenuItem("PLUME/About...", priority = 21)]
        private static void OpenAbout()
        {
            Application.OpenURL("https://www.github.com/liris-xr/PLUME");
        }
    }
}