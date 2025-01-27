using System.Collections.Generic;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity;
using UnityEngine;
using UnityEngine.Scripting;
using static PLUME.Core.Utils.SampleUtils;
using LightmapData = PLUME.Sample.Unity.LightmapData;

namespace PLUME.Base.Module.Unity.Lightmaps
{
    [Preserve]
    public class LightmapsRecorderModule : FrameDataRecorderModule<LightmapsFrameData>
    {
        private LightmapsUpdate _lightmapsUpdateSample;

        protected override void OnStartRecording(RecorderContext ctx)
        {
            base.OnStartRecording(ctx);
            RecordLightmapsUpdate(ctx);
        }

        private void RecordLightmapsUpdate(RecorderContext ctx)
        {
            // TODO: only record the diff instead of the whole Lightmaps
            var safeRefProvider = ctx.SafeRefProvider;
            var s = new LightmapsUpdate();

            s.LightmapsMode = LightmapSettings.lightmapsMode.ToPayload();

            foreach (var lightmap in LightmapSettings.lightmaps)
            {
                var lightmapColorTexture = safeRefProvider.GetOrCreateAssetSafeRef(lightmap.lightmapColor);
                var lightmapDirTexture = safeRefProvider.GetOrCreateAssetSafeRef(lightmap.lightmapDir);
                var lightmapShadowMaskTexture = safeRefProvider.GetOrCreateAssetSafeRef(lightmap.shadowMask);

                var lightmapDataSample = new LightmapData
                {
                    LightmapColorTexture = GetAssetIdentifierPayload(lightmapColorTexture),
                    LightmapDirTexture = GetAssetIdentifierPayload(lightmapDirTexture),
                    LightmapShadowMaskTexture = GetAssetIdentifierPayload(lightmapShadowMaskTexture)
                };
                s.LightmapsData.Add(lightmapDataSample);
            }

            _lightmapsUpdateSample = s;
        }

        protected override LightmapsFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            var lightmapsFrameData = LightmapsFrameData.Pool.Get();
            lightmapsFrameData.SetLightmapsUpdateSample(_lightmapsUpdateSample);
            return lightmapsFrameData;
        }

        protected override void AfterCollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            base.AfterCollectFrameData(frameInfo, ctx);
            _lightmapsUpdateSample = null;
        }
    }
}