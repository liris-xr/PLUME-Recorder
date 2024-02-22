using System;
using PLUME.Core.Settings;
using UnityEngine;

namespace PLUME.Base.Settings
{
    [Serializable]
    public class TransformRecorderModuleSettings : FrameDataRecorderModuleSettings
    {
        public float DistanceThreshold => distanceThreshold;

        [SerializeField] [Tooltip("The minimum distance a transform must move to record an update.")]
        private float distanceThreshold = 0.001f;

        public override void OnValidate()
        {
            if (distanceThreshold < 0)
            {
                distanceThreshold = 0;
            }
        }

        public static TransformRecorderModuleSettings GetOrCreate()
        {
            return GetOrCreate<TransformRecorderModuleSettings>("TransformRecorderModuleSettings");
        }

        protected override string GetSettingsWindowSubPath()
        {
            return "Transform";
        }
    }
}