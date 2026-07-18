// VolumeManager.globalDefaultProfile is core RP 17+ (Unity 6). Guarded so the recorder
// still compiles under Unity 2022 (HDRP 14), where this property does not exist.
#if HDRP_ENABLED && UNITY_6000_0_OR_NEWER
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.Settings;
using UnityEngine.Rendering;
using UnityEngine.Scripting;
using static PLUME.Core.Utils.SampleUtils;

namespace PLUME.Base.Module.Unity.HDRP
{
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
            var defaultVolumeProfile = VolumeManager.instance.globalDefaultProfile;

            if (defaultVolumeProfile != null)
            {
                var defaultVolumeProfileSafeRef = ctx.SafeRefProvider.GetOrCreateAssetSafeRef(defaultVolumeProfile);
                sample.DefaultVolumeProfile = GetAssetIdentifierPayload(defaultVolumeProfileSafeRef);
            }

            _hdrpGlobalSettingsUpdateSample = sample;
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
