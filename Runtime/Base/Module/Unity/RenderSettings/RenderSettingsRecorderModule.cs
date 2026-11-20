using PLUME.Core;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.Settings;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;
using static PLUME.Core.Utils.SampleUtils;

namespace PLUME.Base.Module.Unity
{
    [Preserve]
    public class RenderSettingsRecorderModule : FrameDataRecorderModule<RenderSettingsFrameData>
    {
        private RenderSettingsUpdate _renderSettingsUpdateSample;
        private RecorderContext _ctx;
        
        protected override void OnStartRecording(RecorderContext ctx)
        {
            base.OnStartRecording(ctx);
            _ctx = ctx;

            SceneManager.sceneLoaded += OnLoadScene;
            SceneManager.sceneUnloaded += OnUnloadScene;
            
            RecordRenderSettingsUpdate(ctx);
        }

        protected override void OnStopRecording(RecorderContext ctx)
        {
            base.OnStopRecording(ctx);
            SceneManager.sceneLoaded -= OnLoadScene;
            SceneManager.sceneUnloaded -= OnUnloadScene;
        }

        private void RecordRenderSettingsUpdate(RecorderContext ctx)
        {
            // TODO: only record the diff instead of the whole RenderSettings
            var safeRefProvider = ctx.SafeRefProvider;
            var s = new RenderSettingsUpdate();
            var skybox = safeRefProvider.GetOrCreateAssetSafeRef(RenderSettings.skybox);
            var sun = safeRefProvider.GetOrCreateComponentSafeRef(RenderSettings.sun);
            var customReflectionTexture = safeRefProvider.GetOrCreateAssetSafeRef(RenderSettings.customReflectionTexture);
            
            s.Skybox = GetAssetIdentifierPayload(skybox);
            s.Sun = GetComponentIdentifierPayload(sun);
            s.Fog = RenderSettings.fog;
            s.FogColor = RenderSettings.fogColor.ToPayload();
            s.FogMode = RenderSettings.fogMode.ToPayload();
            s.FogColor = RenderSettings.fogColor.ToPayload();
            s.FogDensity = RenderSettings.fogDensity;
            s.FogStartDistance = RenderSettings.fogStartDistance;
            s.FogEndDistance = RenderSettings.fogEndDistance;
            s.AmbientLightColor = RenderSettings.ambientLight.ToPayload();
            s.AmbientEquatorColor = RenderSettings.ambientEquatorColor.ToPayload();
            s.AmbientGroundColor = RenderSettings.ambientGroundColor.ToPayload();
            s.AmbientSkyColor = RenderSettings.ambientSkyColor.ToPayload();
            s.AmbientIntensity = RenderSettings.ambientIntensity;
            s.AmbientMode = RenderSettings.ambientMode.ToPayload();
            s.AmbientProbe = RenderSettings.ambientProbe.ToPayload();
            s.CustomReflectionTexture = GetAssetIdentifierPayload(customReflectionTexture);
            s.DefaultReflectionMode = RenderSettings.defaultReflectionMode.ToPayload();
            s.DefaultReflectionResolution = RenderSettings.defaultReflectionResolution;
            s.ReflectionBounces = RenderSettings.reflectionBounces;
            s.ReflectionIntensity = RenderSettings.reflectionIntensity;
            s.HaloStrength = RenderSettings.haloStrength;
            s.FlareStrength = RenderSettings.flareStrength;
            s.FlareFadeSpeed = RenderSettings.flareFadeSpeed;
            s.SubtractiveShadowColor = RenderSettings.subtractiveShadowColor.ToPayload();
            
            _renderSettingsUpdateSample = s;
        }
        
        private void OnLoadScene(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
        {
            OnSceneChanged();
        }

        private void OnUnloadScene(UnityEngine.SceneManagement.Scene scene)
        {
            OnSceneChanged();
        }

        // Called from SceneManager events: an exception escaping it would skip the application's own subscribers.
        private void OnSceneChanged()
        {
            if (!_ctx.IsRecordingWith(this))
                return;

            try
            {
                RecordRenderSettingsUpdate(_ctx);
            }
            catch (System.Exception e)
            {
                ErrorReporter.Report($"{nameof(RenderSettingsRecorderModule)}.{nameof(OnSceneChanged)}",
                    "Failed to record the render settings after a scene change. They may be outdated in the record.", e);
            }
        }
        
        protected override RenderSettingsFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            var renderSettingsFrameData = RenderSettingsFrameData.Pool.Get();
            renderSettingsFrameData.SetRenderSettingsUpdateSample(_renderSettingsUpdateSample);
            return renderSettingsFrameData;
        }

        protected override void AfterCollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            base.AfterCollectFrameData(frameInfo, ctx);
            _renderSettingsUpdateSample = null;
        }
    }
}