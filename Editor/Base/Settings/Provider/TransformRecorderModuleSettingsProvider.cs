using PLUME.Base.Settings;
using PLUME.Editor.Core.Settings.Provider;
using UnityEditor;

namespace PLUME.Editor.Base.Settings.Provider
{
    public static class TransformRecorderModuleSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var settings = TransformRecorderModuleSettings.GetOrCreate();
            return RecorderSettingsProvider.CreateSettingsProvider(settings);
        }
    }
}