using System;
using System.IO;

namespace PLUME.Core.Settings
{
    [Serializable]
    public sealed class FrameRecorderModuleSettings : RecorderModuleSettings
    {
        internal static readonly string SettingsWindowPath =
            Path.Join(RecorderSettings.SettingsWindowPath, "Unity Frame Recorder");

        public static FrameRecorderModuleSettings GetOrCreate()
        {
            return GetOrCreateInternal<FrameRecorderModuleSettings>("FrameRecorderModuleSettings");
        }

        public override string GetSettingsWindowPath()
        {
            return SettingsWindowPath;
        }
    }
}