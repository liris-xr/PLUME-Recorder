using UnityEditor;
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