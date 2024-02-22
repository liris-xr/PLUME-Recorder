using System;
using System.IO;

namespace PLUME.Core.Settings
{
    [Serializable]
    public abstract class FrameDataRecorderModuleSettings : RecorderModuleSettings
    {
        protected static T GetOrCreate<T>(string path) where T : FrameDataRecorderModuleSettings
        {
            return GetOrCreateInternal<T>(Path.Join("FrameDataRecorderModules", path));
        }
        
        internal sealed override string GetSettingsWindowPath()
        {
            return Path.Join(FrameRecorderModuleSettings.SettingsWindowPath, GetSettingsWindowSubPath());
        }

        protected abstract string GetSettingsWindowSubPath();
    }
}