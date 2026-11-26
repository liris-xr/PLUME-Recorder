#if HDRP_ENABLED
using System.Reflection;
using PLUME.Core;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.Settings;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Scripting;
using static PLUME.Core.Utils.SampleUtils;

namespace PLUME.Base.Module.Unity.HDRP
{
    /// <summary>
    /// Records HDRP's default volume profile, which gives the sky, exposure and other volume defaults of every scene.
    /// </summary>
    [Preserve]
    public class HDRPGlobalSettingsRecorderModule : FrameDataRecorderModule<HDRPGlobalSettingsFrameData>
    {
        private HDRPGlobalSettingsUpdate _hdrpGlobalSettingsUpdateSample;

        protected override void OnStartRecording(RecorderContext ctx)
        {
            base.OnStartRecording(ctx);
            RecordHDRPGlobalSettingsUpdate(ctx);
        }

        private void RecordHDRPGlobalSettingsUpdate(RecorderContext ctx)
        {
            var sample = new HDRPGlobalSettingsUpdate();
            var defaultVolumeProfile = GetDefaultVolumeProfile();

            if (defaultVolumeProfile != null)
            {
                var defaultVolumeProfileSafeRef = ctx.SafeRefProvider.GetOrCreateAssetSafeRef(defaultVolumeProfile);
                sample.DefaultVolumeProfile = GetAssetIdentifierPayload(defaultVolumeProfileSafeRef);
            }

            _hdrpGlobalSettingsUpdateSample = sample;
        }

#if !UNITY_6000_0_OR_NEWER
        // HDRP 14 (Unity 2022) keeps the profile in its internal global settings. Unity 2022 is an LTS line: this
        // internal API no longer changes. Not found, the profile is not recorded and the replay uses its own.
        private static readonly PropertyInfo VolumeProfileProperty = typeof(HDRenderPipeline).Assembly
            .GetType("UnityEngine.Rendering.HighDefinition.HDRenderPipelineGlobalSettings")
            ?.GetProperty("volumeProfile", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
#endif

        /// <summary>
        /// HDRP's default volume profile, typed as an Object so that the editor (bundle builder) can use it without
        /// referencing the render pipeline assemblies.
        /// </summary>
        public static UnityEngine.Object GetDefaultVolumeProfile()
        {
            // HDRP installed but another pipeline active: its profile is not the one rendering.
            if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline is not HDRenderPipelineAsset)
                return null;

#if UNITY_6000_0_OR_NEWER
            // Set once HDRP initialized its volumes: unset (e.g. in batch mode), the replay uses its own profile.
            return VolumeManager.instance.globalDefaultProfile;
#else
            if (VolumeProfileProperty == null)
            {
                ErrorReporter.Warn($"{nameof(HDRPGlobalSettingsRecorderModule)}.VolumeProfileProperty",
                    "HDRP's default volume profile cannot be read in this HDRP version: the replay uses its own sky " +
                    "and exposure defaults.");
                return null;
            }

            var globalSettings = UnityEngine.Rendering.GraphicsSettings.GetSettingsForRenderPipeline<HDRenderPipeline>();
            return globalSettings == null ? null : VolumeProfileProperty.GetValue(globalSettings) as VolumeProfile;
#endif
        }

        protected override HDRPGlobalSettingsFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            var frameData = HDRPGlobalSettingsFrameData.Pool.Get();
            frameData.SetHDRPGlobalSettingsUpdateSample(_hdrpGlobalSettingsUpdateSample);
            return frameData;
        }

        protected override void AfterCollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            base.AfterCollectFrameData(frameInfo, ctx);
            _hdrpGlobalSettingsUpdateSample = null;
        }
    }
}
#endif
