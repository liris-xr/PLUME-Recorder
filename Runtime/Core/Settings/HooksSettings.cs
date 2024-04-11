using System;
using System.IO;
using UnityEngine;

namespace PLUME.Core.Settings
{
    [Serializable]
    public sealed class HooksSettings : Settings
    {
        public string[] InjectedAssemblies => injectedAssemblies;
        
        [SerializeField]
        private string[] injectedAssemblies = {
            "Assembly-CSharp.dll",
            "Unity.XR.Interaction.Toolkit.dll",
            "Unity.XR.Interaction.Toolkit.Samples.StarterAssets.dll",
            "Unity.VisualScripting.Core.dll"
        };
        
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