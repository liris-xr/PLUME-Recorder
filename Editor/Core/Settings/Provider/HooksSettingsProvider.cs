using PLUME.Core.Settings;
using UnityEditor;

namespace PLUME.Editor.Core.Settings.Provider
{
    public static class RecorderSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var settings = SettingsEditor.GetSettings<RecorderSettings>();
            return CreateSettingsProvider(settings);
        }

        public static SettingsProvider CreateSettingsProvider<T>(T settings) where T : PLUME.Core.Settings.Settings
        {
            var serializedObject = new SerializedObject(settings);
            var provider = AssetSettingsProvider.CreateProviderFromObject(settings.GetSettingsWindowPath(), settings);
            provider.keywords = SettingsProvider.GetSearchKeywordsFromSerializedObject(serializedObject);
            return provider;
        }
    }
}