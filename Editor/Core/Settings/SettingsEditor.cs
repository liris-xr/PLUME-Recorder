using PLUME.Core.Settings;

namespace PLUME.Editor.Core.Settings
{
    public abstract class SettingsEditor : UnityEditor.Editor
    {
        internal static readonly FileSettingsProvider SettingsProvider = new();

        public static TV GetSettings<TV>() where TV : PLUME.Core.Settings.Settings
        {
            return SettingsProvider.GetOrCreate<TV>();
        }
    }
    
    public abstract class SettingsEditor<T> : SettingsEditor where T : PLUME.Core.Settings.Settings
    {
        protected T GetSettings()
        {
            return SettingsProvider.GetOrCreate<T>();
        }
    }
}