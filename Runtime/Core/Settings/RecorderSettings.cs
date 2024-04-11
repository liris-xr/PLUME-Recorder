using System;
using UnityEngine;

namespace PLUME.Core.Settings
{
    [Serializable]
    public sealed class RecorderSettings : Settings
    {
        internal const string SettingsWindowPath = "Project/PLUME Recorder";

        public bool StartOnPlay => startOnPlay;
        
        public string DefaultRecordPrefix => defaultRecordPrefix;
        
        public string DefaultRecordExtraMetadata => defaultRecordExtraMetadata;

        [SerializeField] [Tooltip("If true, the recorder will start recording as soon as the game starts.")]
        private bool startOnPlay = true;
        
        [SerializeField] [Tooltip("Default name of the records. Will be suffixed with the date.")]
        private string defaultRecordPrefix = "record";
        
        [SerializeField] [Tooltip("Extra information that might be relevant to integrate in every record.")]
        private string defaultRecordExtraMetadata = "";

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