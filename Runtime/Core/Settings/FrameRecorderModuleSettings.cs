using System;
using System.IO;
using UnityEngine;

namespace PLUME.Core.Settings
{
    [Serializable]
    public sealed class FrameRecorderModuleSettings : RecorderModuleSettings
    {
        internal static readonly string SettingsWindowPath =
            Path.Join(RecorderSettings.SettingsWindowPath, "Unity Frame Recorder");
        
        public float UpdateRate => updateRate;
        
        [SerializeField]
        [Tooltip("Determines the maximum number of frames per second that the Unity frame recorder will capture.")]
        internal float updateRate = 140;

        public override void OnValidate()
        {
            if (updateRate < 0)
            {
                Debug.LogWarning("Update rate cannot be negative. Setting to 0.");
                updateRate = 0;
            }
        }
        
        public static FrameRecorderModuleSettings GetOrCreate()
        {
            return GetOrCreateInternal<FrameRecorderModuleSettings>("FrameRecorderModuleSettings");
        }

        internal override string GetSettingsWindowPath()
        {
            return SettingsWindowPath;
        }
    }
}