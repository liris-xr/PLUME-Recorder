using System.Collections.Generic;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity;
using UnityEngine.Scripting;
using TerrainSafeRef = PLUME.Core.Object.SafeRef.IComponentSafeRef<UnityEngine.Terrain>;
using static PLUME.Core.Utils.SampleUtils;

namespace PLUME.Base.Module.Unity.Terrain
{
    [Preserve]
    public class TerrainRecorderModule : ComponentRecorderModule<UnityEngine.Terrain, TerrainFrameData>
    {
        private readonly Dictionary<TerrainSafeRef, TerrainCreate> _createSamples = new();
        private readonly Dictionary<TerrainSafeRef, TerrainDestroy> _destroySamples = new();
        private readonly Dictionary<TerrainSafeRef, TerrainUpdate> _updateSamples = new();

        protected override void OnObjectMarkedCreated(TerrainSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);

            var terrain = objSafeRef.Component;
            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.Enabled = objSafeRef.Component.enabled;
            updateSample.TerrainData = GetAssetIdentifierPayload(terrain.terrainData);
            updateSample.TreeDistance = terrain.treeDistance;
            updateSample.TreeBillboardDistance = terrain.treeBillboardDistance;
            updateSample.TreeCrossFadeLength = terrain.treeCrossFadeLength;
            updateSample.TreeMaximumFullLodCount = terrain.treeMaximumFullLODCount;
            updateSample.DetailObjectDistance = terrain.detailObjectDistance;
            updateSample.DetailObjectDensity = terrain.detailObjectDensity;
            updateSample.HeightmapPixelError = terrain.heightmapPixelError;
            updateSample.HeightmapMaximumLod = terrain.heightmapMaximumLOD;
            updateSample.BasemapDistance = terrain.basemapDistance;
            updateSample.LightmapIndex = terrain.lightmapIndex;
            updateSample.RealtimeLightmapIndex = terrain.realtimeLightmapIndex;
            updateSample.LightmapScaleOffset = terrain.lightmapScaleOffset.ToPayload();
            updateSample.RealtimeLightmapScaleOffset = terrain.realtimeLightmapScaleOffset.ToPayload();
            updateSample.KeepUnusedRenderingResources = terrain.keepUnusedRenderingResources;
            updateSample.ShadowCastingMode = terrain.shadowCastingMode.ToPayload();
            updateSample.ReflectionProbeUsage = terrain.reflectionProbeUsage.ToPayload();
            updateSample.MaterialTemplate = GetAssetIdentifierPayload(terrain.materialTemplate);
            updateSample.DrawHeightmap = terrain.drawHeightmap;
            updateSample.AllowAutoConnect = terrain.allowAutoConnect;
            updateSample.GroupingId = terrain.groupingID;
            updateSample.DrawInstanced = terrain.drawInstanced;
            updateSample.NormalmapTexture = GetAssetIdentifierPayload(terrain.normalmapTexture);
            updateSample.DrawTreesAndFoliage = terrain.drawTreesAndFoliage;
            updateSample.PatchBoundsMultiplier = terrain.patchBoundsMultiplier.ToPayload();
            updateSample.TreeLodBiasMultiplier = terrain.treeLODBiasMultiplier;
            updateSample.CollectDetailPatches = terrain.collectDetailPatches;

            _createSamples[objSafeRef] = new TerrainCreate { Component = GetComponentIdentifierPayload(objSafeRef) };
        }

        protected override void OnObjectMarkedDestroyed(TerrainSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedDestroyed(objSafeRef, ctx);
            _destroySamples[objSafeRef] = new TerrainDestroy { Component = GetComponentIdentifierPayload(objSafeRef) };
        }

        protected override TerrainFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            var frameData = TerrainFrameData.Pool.Get();
            frameData.AddCreateSamples(_createSamples.Values);
            frameData.AddDestroySamples(_destroySamples.Values);
            frameData.AddUpdateSamples(_updateSamples.Values);
            return frameData;
        }

        private TerrainUpdate GetOrCreateUpdateSample(TerrainSafeRef objSafeRef)
        {
            if (_updateSamples.TryGetValue(objSafeRef, out var sample))
                return sample;
            _updateSamples[objSafeRef] = new TerrainUpdate { Component = GetComponentIdentifierPayload(objSafeRef) };
            return _updateSamples[objSafeRef];
        }

        protected override void AfterCollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            base.AfterCollectFrameData(frameInfo, ctx);
            _createSamples.Clear();
            _destroySamples.Clear();
            _updateSamples.Clear();
        }
    }
}