using PLUME.Core.Settings;
using UnityEditor;

namespace PLUME.Editor.Core.Settings.Provider
{
    public static class FrameRecorderModuleSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var settings = SettingsEditor.GetSettings<FrameRecorderModuleSettings>();
            return RecorderSettingsProvider.CreateSettingsProvider(settings);
        }
    }
}