using UnityEditor;

namespace PLUME.Editor
{
    public static class MenuItems
    {
        [MenuItem("PLUME/Build Asset Bundle")]
        private static void BuildAssetBundle()
        {
            AssetBundleBuilder.BuildAssetBundle();
        }
    }
}