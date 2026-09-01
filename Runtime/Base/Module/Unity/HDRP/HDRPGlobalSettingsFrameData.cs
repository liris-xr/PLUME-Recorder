#if HDRP_ENABLED
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.Settings;

namespace PLUME.Base.Module.Unity.HDRP
{
    public class HDRPGlobalSettingsFrameData : PooledFrameData<HDRPGlobalSettingsFrameData>
    {
        public static readonly FrameDataPool<HDRPGlobalSettingsFrameData> Pool = new();

        private HDRPGlobalSettingsUpdate _hdrpGlobalSettingsUpdateSample;

        public void SetHDRPGlobalSettingsUpdateSample(HDRPGlobalSettingsUpdate hdrpGlobalSettingsUpdateSample)
        {
            _hdrpGlobalSettingsUpdateSample = hdrpGlobalSettingsUpdateSample;
        }

        public override void Serialize(FrameDataWriter frameDataWriter)
        {
            if (_hdrpGlobalSettingsUpdateSample != null)
                frameDataWriter.WriteManaged(_hdrpGlobalSettingsUpdateSample);
        }

        public override void Clear()
        {
            _hdrpGlobalSettingsUpdateSample = null;
        }
    }
}
#endif
