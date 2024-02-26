using System;
using System.IO;
using PLUME.Core.Settings;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PLUME.Base.Settings
{
    [Serializable]
    public class GameObjectRecorderModuleSettings : FrameDataRecorderModuleSettings
    {
        public ResolvedChangeDetectionMode ResolvedChangeDetectionMode =>
            changeDetectionMode.ResolveForCurrentRuntimePlatform();

        [SerializeField] private ChangeDetectionMode changeDetectionMode = ChangeDetectionMode.Auto;

        public override void OnValidate()
        {
#if UNITY_EDITOR
            if (!changeDetectionMode.IsSupportedForActiveBuildTarget())
            {
                var message =
                    $"GameObject change detection mode {changeDetectionMode} is not supported for build target {EditorUserBuildSettings.activeBuildTarget}";

                if (Plume.IsBuilding)
                {
                    throw new NotSupportedException(message);
                }

                Debug.LogWarning(message);
            }
#endif
        }

        internal override string GetSettingsFileName()
        {
            return "GameObjectRecorderModuleSettings";
        }

        internal override string GetSettingsWindowPath()
        {
            return Path.Join(base.GetSettingsWindowPath(), "GameObject");
        }
    }
}