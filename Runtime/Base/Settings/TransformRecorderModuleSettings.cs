using System;
using System.IO;
using PLUME.Core.Settings;
using UnityEngine;

namespace PLUME.Base.Settings
{
    [Serializable]
    public class TransformRecorderModuleSettings : FrameDataRecorderModuleSettings
    {
        public float PositionThreshold => positionThreshold;

        public float ScaleThreshold => scaleThreshold;

        public float AngularThreshold => angularThreshold;

        [SerializeField] [Tooltip("The minimum position change a transform must undergo to record an update.")]
        private float positionThreshold = 0.001f;

        [SerializeField] [Tooltip("The minimum scale change a transform must undergo to record an update.")]
        private float scaleThreshold = 0.001f;

        [SerializeField]
        [Tooltip("The minimum angular change a transform must undergo to record an update. Expressed in radians.")]
        private float angularThreshold = 0.001f;

        public override void OnValidate()
        {
            if (positionThreshold < 0)
            {
                positionThreshold = 0;
            }

            if (scaleThreshold < 0)
            {
                scaleThreshold = 0;
            }

            if (angularThreshold < 0)
            {
                angularThreshold = 0;
            }
        }

        internal override string GetSettingsFileName()
        {
            return "TransformRecorderModuleSettings";
        }

        internal override string GetSettingsWindowPath()
        {
            return Path.Join(base.GetSettingsWindowPath(), "Transform");
        }
    }
}