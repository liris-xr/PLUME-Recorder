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

        public float FrameRecordingRate => frameRecordingRate;

        [SerializeField] [Tooltip("Maximum number of frames recorded per second.")]
        internal float frameRecordingRate = 140;

        private void OnValidate()
        {
            if(frameRecordingRate < 0)
                frameRecordingRate = 1;
        }

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