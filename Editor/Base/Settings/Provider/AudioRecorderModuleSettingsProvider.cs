using PLUME.Base.Settings;
using PLUME.Editor.Core.Settings;
using PLUME.Editor.Core.Settings.Provider;
using UnityEditor;

namespace PLUME.Editor.Base.Settings.Provider
{
    public static class AudioRecorderModuleSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var settings = SettingsEditor.GetSettings<AudioRecorderModuleSettings>();
            return RecorderSettingsProvider.CreateSettingsProvider(settings);
        }
    }
}