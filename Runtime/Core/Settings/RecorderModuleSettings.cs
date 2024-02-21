using System;
using UnityEngine;

namespace PLUME.Core.Settings
{
    [Serializable]
    public abstract class RecorderModuleSettings : Settings
    {
        public bool Enabled => enabled;

        [SerializeField] [Tooltip("If false, the module will not be created and attached to the recorder.")]
        internal bool enabled = true;
    }
}