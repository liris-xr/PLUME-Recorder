using System;

namespace PLUME.Core.Settings
{
    [Serializable]
    public abstract class RecorderModuleSettings : Settings
    {
        internal override string GetSettingsWindowPath()
        {
            return RecorderSettings.SettingsWindowPath;
        }
    }
}