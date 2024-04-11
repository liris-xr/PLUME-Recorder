using System;
using System.IO;
using UnityEngine;

namespace PLUME.Core.Settings
{
    [Serializable]
    public sealed class FrameRecorderModuleSettings : RecorderModuleSettings
    {
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

        internal override string GetSettingsFileName()
        {
            return "FrameRecorderModuleSettings";
        }

        internal override string GetSettingsWindowPath()
        {
            return Path.Join(base.GetSettingsWindowPath(), "Frame Recorder");
        }
    }
}