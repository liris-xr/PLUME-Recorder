using System;
using System.IO;
using PLUME.Core.Settings;
using UnityEngine;

namespace PLUME.Base.Settings
{
    [Serializable]
    public class LslRecorderModuleSettings : RecorderModuleSettings
    {
        public string ResolverPredicate => resolverPredicate;
        
        public int ResolveInterval => resolveInterval;
        
        public float ForgetAfter => forgetAfter;

        [SerializeField] [Tooltip("XPath predicate used when resolving LSL streams")]
        private string resolverPredicate = "*";
        
        [SerializeField] [Tooltip("Time in milliseconds between LSL stream resolution attempts.")]
        private int resolveInterval = 100;
        
        [SerializeField] [Tooltip("Time in seconds after which a stream is assumed to be lost.")]
        private float forgetAfter = 5.0f;

        public override void OnValidate()
        {
            if (resolveInterval < 0)
            {
                resolveInterval = 0;
            }
            
            if (forgetAfter < 0)
            {
                forgetAfter = 0;
            }
        }

        internal override string GetSettingsFileName()
        {
            return "LslRecorderModuleSettings";
        }

        internal override string GetSettingsWindowPath()
        {
            return Path.Join(base.GetSettingsWindowPath(), "LabStreamingLayer");
        }
    }
}