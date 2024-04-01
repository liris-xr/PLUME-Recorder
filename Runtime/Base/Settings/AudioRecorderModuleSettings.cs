using System;
using System.IO;
using PLUME.Core.Settings;
using UnityEngine;

namespace PLUME.Base.Settings
{
    [Serializable]
    public class AudioRecorderModuleSettings : RecorderModuleSettings
    {
        public bool Enabled => enabled;
        
        public bool LogSilenceInsertion => logSilenceInsertion;

        [SerializeField] [Tooltip("If true, will record the audio in a WAV file.")]
        private bool enabled = false;
        
        [SerializeField] [Tooltip("If true, silence insertion in WAV will be logged.")]
        private bool logSilenceInsertion;

        internal override string GetSettingsFileName()
        {
            return "AudioRecorderModuleSettings";
        }

        internal override string GetSettingsWindowPath()
        {
            return Path.Join(base.GetSettingsWindowPath(), "Audio");
        }
    }
}