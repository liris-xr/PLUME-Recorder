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

        public static TransformRecorderModuleSettings GetOrCreate()
        {
            return GetOrCreate<TransformRecorderModuleSettings>("TransformRecorderModuleSettings");
        }

        public override string GetSettingsWindowSubPath()
        {
            return "Transform";
        }
    }
}