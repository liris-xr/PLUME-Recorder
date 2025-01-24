using System;
using System.IO;
using UnityEngine;

namespace PLUME.Core.Settings
{
    [Serializable]
    public sealed class HooksSettings : Settings
    {
        public string[] BlacklistedAssemblyNames => blacklistedAssemblyNames;

        [Tooltip("Assembly names (as defined in the asmdef, without .dll) that should not be injected with hooks.")]
        [SerializeField]
        private string[] blacklistedAssemblyNames = Array.Empty<string>();

        internal override string GetSettingsFileName()
        {
            return "HooksSettings";
        }

        internal override string GetSettingsWindowPath()
        {
            return Path.Join(RecorderSettings.SettingsWindowPath, "Hooks");
        }
    }
}