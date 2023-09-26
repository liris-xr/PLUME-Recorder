using PLUME.Sample.Unity;
using UnityEngine;

namespace PLUME
{
    public class RenderSettingsRecorderModule : RecorderModule, IStartRecordingEventReceiver
    {
        public new void OnStartRecording()
        {
            base.OnStartRecording();
            var renderSettingsUpdate = new RenderSettingsUpdate
            {
                SkyboxId = RenderSettings.skybox == null ? null : RenderSettings.skybox.ToAssetIdentifierPayload(),
                AmbientEquatorColor = RenderSettings.ambientEquatorColor.ToPayload(),
                AmbientGroundColor = RenderSettings.ambientGroundColor.ToPayload(),
                AmbientIntensity = RenderSettings.ambientIntensity,
                AmbientLight = RenderSettings.ambientLight.ToPayload(),
                AmbientMode = RenderSettings.ambientMode.ToPayload(),
                AmbientProbe = RenderSettings.ambientProbe.ToPayload(),
                AmbientSkyColor = RenderSettings.ambientSkyColor.ToPayload(),
#if UNITY_2022_1_OR_NEWER
                CustomReflectionId = RenderSettings.customReflectionTexture == null
                    ? null
                    : RenderSettings.customReflectionTexture.ToAssetIdentifierPayload(),
#else
                CustomReflectionId = RenderSettings.customReflection == null
                    ? null
                    : RenderSettings.customReflection.ToAssetIdentifierPayload(),
#endif
                DefaultReflectionMode = RenderSettings.defaultReflectionMode.ToPayload(),
                DefaultReflectionResolution = RenderSettings.defaultReflectionResolution,
                FlareFadeSpeed = RenderSettings.flareFadeSpeed,
                FlareStrength = RenderSettings.flareStrength,
                Fog = RenderSettings.fog,
                FogColor = RenderSettings.fogColor.ToPayload(),
                FogDensity = RenderSettings.fogDensity,
                FogEndDistance = RenderSettings.fogEndDistance,
                FogMode = RenderSettings.fogMode.ToPayload(),
                FogStartDistance = RenderSettings.fogStartDistance,
                HaloStrength = RenderSettings.haloStrength,
                ReflectionBounces = RenderSettings.reflectionBounces,
                ReflectionIntensity = RenderSettings.reflectionIntensity,
                SubtractiveShadowColor = RenderSettings.subtractiveShadowColor.ToPayload(),
                SunId = RenderSettings.sun == null ? null : RenderSettings.sun.ToIdentifierPayload()
            };

            recorder.RecordSampleStamped(renderSettingsUpdate);
        }

        protected override void ResetCache()
        {
        }
    }
}