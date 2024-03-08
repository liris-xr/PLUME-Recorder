using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PLUME.Core.Settings
{
    public enum ChangeDetectionMode
    {
        Auto,
        Manual,
        Polling,
        Event
    }

    public enum ResolvedChangeDetectionMode
    {
        Manual,
        Polling,
        Event
    }

    public static class ChangeDetectionModeExtensions
    {
        public static ResolvedChangeDetectionMode ResolveAutoModeForCurrentRuntimePlatform()
        {
            return ResolveAutoModeForPlatform(Application.platform);
        }

        public static ResolvedChangeDetectionMode ResolveAutoModeForPlatform(RuntimePlatform platform)
        {
            var isArm = platform is RuntimePlatform.tvOS or RuntimePlatform.IPhonePlayer or RuntimePlatform.Android
                or RuntimePlatform.WSAPlayerARM or RuntimePlatform.EmbeddedLinuxArm32
                or RuntimePlatform.EmbeddedLinuxArm64 or RuntimePlatform.QNXArm32 or RuntimePlatform.QNXArm64;

            return isArm ? ResolvedChangeDetectionMode.Polling : ResolvedChangeDetectionMode.Event;
        }

        public static ResolvedChangeDetectionMode ResolveForCurrentRuntimePlatform(this ChangeDetectionMode mode)
        {
            return mode switch
            {
                ChangeDetectionMode.Auto => ResolveAutoModeForCurrentRuntimePlatform(),
                ChangeDetectionMode.Manual => ResolvedChangeDetectionMode.Manual,
                ChangeDetectionMode.Polling => ResolvedChangeDetectionMode.Polling,
                ChangeDetectionMode.Event => ResolvedChangeDetectionMode.Event,
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            };
        }
        
        public static bool IsSupportedForPlatform(this ChangeDetectionMode mode, RuntimePlatform platform)
        {
            var isArm = platform is RuntimePlatform.tvOS or RuntimePlatform.IPhonePlayer or RuntimePlatform.Android
                or RuntimePlatform.WSAPlayerARM or RuntimePlatform.EmbeddedLinuxArm32
                or RuntimePlatform.EmbeddedLinuxArm64 or RuntimePlatform.QNXArm32 or RuntimePlatform.QNXArm64;

            switch (mode)
            {
                case ChangeDetectionMode.Event:
                    return !isArm;
                case ChangeDetectionMode.Auto:
                case ChangeDetectionMode.Manual:
                case ChangeDetectionMode.Polling:
                default:
                    return true;
            }
        }

#if UNITY_EDITOR
        public static bool IsSupportedForActiveBuildTarget(this ChangeDetectionMode mode)
        {
            return mode.IsSupportedForPlatform(EditorUserBuildSettings.activeBuildTarget);
        }

        public static bool IsSupportedForPlatform(this ChangeDetectionMode mode, BuildTarget platform)
        {
            var isArm = platform is BuildTarget.tvOS or BuildTarget.iOS or BuildTarget.Android;

            switch (mode)
            {
                case ChangeDetectionMode.Event:
                    return !isArm;
                case ChangeDetectionMode.Auto:
                case ChangeDetectionMode.Manual:
                case ChangeDetectionMode.Polling:
                default:
                    return true;
            }
        }
#endif
    }
}