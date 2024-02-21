using System;
using System.IO;

namespace PLUME.Core.Settings
{
    [Serializable]
    public abstract class FrameDataRecorderModuleSettings : RecorderModuleSettings
    {
        public static T GetOrCreate<T>(string path) where T : FrameDataRecorderModuleSettings
        {
            return GetOrCreateInternal<T>(Path.Join("FrameDataRecorderModules", path));
        }
        
        public override string GetSettingsWindowPath()
        {
            return Path.Join(FrameRecorderModuleSettings.SettingsWindowPath, GetSettingsWindowSubPath());
        }

        public abstract string GetSettingsWindowSubPath();
    }
}