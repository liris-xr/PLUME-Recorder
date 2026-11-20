#if HDRP_ENABLED
using PLUME.Core.Utils;
using PLUME.Sample.Unity.HDRP;
using UnityEngine.Rendering.HighDefinition;

namespace PLUME.Base.Module.Unity.HDRP
{
    public static class HDRPSampleUtils
    {
        public static HDClearColorMode ToPayload(this HDAdditionalCameraData.ClearColorMode clearColorMode)
        {
            return clearColorMode switch
            {
                HDAdditionalCameraData.ClearColorMode.Sky => HDClearColorMode.Sky,
                HDAdditionalCameraData.ClearColorMode.Color => HDClearColorMode.Color,
                HDAdditionalCameraData.ClearColorMode.None => HDClearColorMode.None,
                _ => SampleUtils.UnknownEnumValue<HDAdditionalCameraData.ClearColorMode, HDClearColorMode>(clearColorMode)
            };
        }

        public static HDCameraAntialiasingMode ToPayload(this HDAdditionalCameraData.AntialiasingMode antialiasingMode)
        {
            return antialiasingMode switch
            {
                HDAdditionalCameraData.AntialiasingMode.None => HDCameraAntialiasingMode.None,
                HDAdditionalCameraData.AntialiasingMode.FastApproximateAntialiasing => HDCameraAntialiasingMode
                    .FastApproximate,
                HDAdditionalCameraData.AntialiasingMode.TemporalAntialiasing => HDCameraAntialiasingMode.Temporal,
                HDAdditionalCameraData.AntialiasingMode.SubpixelMorphologicalAntiAliasing => HDCameraAntialiasingMode
                    .SubpixelMorphological,
                _ => SampleUtils.UnknownEnumValue<HDAdditionalCameraData.AntialiasingMode, HDCameraAntialiasingMode>(antialiasingMode)
            };
        }

        public static HDSMAAQualityLevel ToPayload(this HDAdditionalCameraData.SMAAQualityLevel smaaQualityLevel)
        {
            return smaaQualityLevel switch
            {
                HDAdditionalCameraData.SMAAQualityLevel.Low => HDSMAAQualityLevel.Low,
                HDAdditionalCameraData.SMAAQualityLevel.Medium => HDSMAAQualityLevel.Medium,
                HDAdditionalCameraData.SMAAQualityLevel.High => HDSMAAQualityLevel.High,
                _ => SampleUtils.UnknownEnumValue<HDAdditionalCameraData.SMAAQualityLevel, HDSMAAQualityLevel>(smaaQualityLevel)
            };
        }

        public static HDTAAQualityLevel ToPayload(this HDAdditionalCameraData.TAAQualityLevel taaQualityLevel)
        {
            return taaQualityLevel switch
            {
                HDAdditionalCameraData.TAAQualityLevel.Low => HDTAAQualityLevel.Low,
                HDAdditionalCameraData.TAAQualityLevel.Medium => HDTAAQualityLevel.Medium,
                HDAdditionalCameraData.TAAQualityLevel.High => HDTAAQualityLevel.High,
                _ => SampleUtils.UnknownEnumValue<HDAdditionalCameraData.TAAQualityLevel, HDTAAQualityLevel>(taaQualityLevel)
            };
        }
    }
}
#endif
