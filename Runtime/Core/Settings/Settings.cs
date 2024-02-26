using System;
using UnityEngine;

namespace PLUME.Core.Settings
{
    [Serializable]
    public abstract class Settings : ScriptableObject
    {
        public virtual void OnValidate()
        {
        }

        internal abstract string GetSettingsFileName();

        internal abstract string GetSettingsWindowPath();
    }
}