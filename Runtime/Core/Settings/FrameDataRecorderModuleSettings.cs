using System;
using System.IO;

namespace PLUME.Core.Settings
{
    [Serializable]
    public abstract class FrameDataRecorderModuleSettings : RecorderModuleSettings
    {
        internal override string GetSettingsWindowPath()
        {
            return Path.Join(base.GetSettingsWindowPath(), "Frame Recorder");
        }
    }
}