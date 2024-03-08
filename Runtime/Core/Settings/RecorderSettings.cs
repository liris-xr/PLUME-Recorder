using System;
using UnityEngine;

namespace PLUME.Core.Settings
{
    [Serializable]
    public sealed class RecorderSettings : Settings
    {
        internal const string SettingsWindowPath = "Project/PLUME Recorder";

        public bool StartOnPlay => startOnPlay;

        [SerializeField] [Tooltip("If true, the recorder will start recording as soon as the game starts.")]
        private bool startOnPlay = true;

        internal override string GetSettingsFileName()
        {
            return "RecorderSettings";
        }

        internal override string GetSettingsWindowPath()
        {
            return SettingsWindowPath;
        }
    }
}