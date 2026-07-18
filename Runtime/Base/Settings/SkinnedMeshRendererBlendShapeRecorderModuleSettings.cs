using System;
using System.IO;
using PLUME.Core.Settings;
using UnityEngine;

namespace PLUME.Base.Settings
{
    [Serializable]
    public class SkinnedMeshRendererBlendShapeRecorderModuleSettings : FrameDataRecorderModuleSettings
    {
        public float WeightThreshold => weightThreshold;

        [SerializeField]
        [Tooltip(
            "The minimum change of any single blend shape weight required to record an update for a renderer. " +
            "Unity blend shape weights are conventionally in the 0-100 range.")]
        private float weightThreshold = 0.01f;

        public override void OnValidate()
        {
            if (weightThreshold < 0)
            {
                weightThreshold = 0;
            }
        }

        internal override string GetSettingsFileName()
        {
            return "SkinnedMeshRendererBlendShapeRecorderModuleSettings";
        }

        internal override string GetSettingsWindowPath()
        {
            return Path.Join(base.GetSettingsWindowPath(), "Skinned Mesh Renderer Blend Shape");
        }
    }
}
