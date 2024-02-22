using System;
using UnityEngine;

namespace PLUME.Core.Settings
{
    [Serializable]
    public sealed class RecorderSettings : Settings
    {
        internal const string SettingsWindowPath = "Project/PLUME Recorder";

        public bool StartOnPlay => startOnPlay;

        public float UpdateRate => updateRate;

        [SerializeField] [Tooltip("If true, the recorder will start recording as soon as the game starts.")]
        private bool startOnPlay = true;

        [SerializeField]
        [Tooltip("Determines the target number of recorder updates per second. " +
                 "This drives all the recorder modules update rates.")]
        internal float updateRate = 140;

        private void OnValidate()
        {
            if (updateRate < 0)
                updateRate = 0;
        }

        public static RecorderSettings GetOrCreate()
        {
            return GetOrCreateInternal<RecorderSettings>("RecorderSettings");
        }

        public override string GetSettingsWindowPath()
        {
            return SettingsWindowPath;
        }
    }
}