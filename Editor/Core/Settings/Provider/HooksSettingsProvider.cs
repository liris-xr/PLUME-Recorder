using PLUME.Core.Settings;
using UnityEditor;

namespace PLUME.Editor.Core.Settings.Provider
{
    public static class HooksSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var settings = SettingsEditor.GetSettings<HooksSettings>();
            return RecorderSettingsProvider.CreateSettingsProvider(settings);
        }
    }
}