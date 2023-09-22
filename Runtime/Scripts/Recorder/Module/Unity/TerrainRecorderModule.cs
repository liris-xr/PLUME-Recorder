using System.Collections.Generic;
using PLUME.Sample.Unity;
using UnityEngine;

namespace PLUME
{
    public class TerrainRecorderModule : RecorderModule, IStartRecordingObjectEventReceiver,
        IStopRecordingObjectEventReceiver
    {
        private readonly Dictionary<int, Terrain> _recordedTerrains = new();
        private readonly Dictionary<int, bool> _lastEnabled = new();


        protected override void ResetCache()
        {
            _recordedTerrains.Clear();
            _lastEnabled.Clear();
        }

        public void OnStartRecordingObject(Object obj)
        {
            // ReSharper disable once LocalVariableHidesMember
            if (obj is Terrain terrain && !_recordedTerrains.ContainsKey(terrain.GetInstanceID()))
            {
                var terrainInstanceId = terrain.GetInstanceID();
                _recordedTerrains.Add(terrainInstanceId, terrain);
                _lastEnabled.Add(terrainInstanceId, terrain.enabled);
                RecordCreation(terrain);
            }
        }

        public void OnStopRecordingObject(int objectInstanceId)
        {
            if (_recordedTerrains.ContainsKey(objectInstanceId))
            {
                RecordDestruction(objectInstanceId);
                RemoveFromCache(objectInstanceId);
            }
        }

        private void RemoveFromCache(int terrainInstanceId)
        {
            _recordedTerrains.Remove(terrainInstanceId);
            _lastEnabled.Remove(terrainInstanceId);
        }

        public void FixedUpdate()
        {
            var nullTerrainsInstanceIds = new List<int>();

            foreach (var (terrainInstanceId, terrain) in _recordedTerrains)
            {
                if (terrain == null)
                {
                    nullTerrainsInstanceIds.Add(terrainInstanceId);
                    continue;
                }

                if (_lastEnabled[terrainInstanceId] != terrain.enabled)
                {
                    _lastEnabled[terrainInstanceId] = terrain.enabled;

                    var terrainUpdateEnabled = new TerrainUpdateEnabled
                    {
                        Id = terrain.ToIdentifierPayload(),
                        Enabled = terrain.enabled
                    };

                    recorder.RecordSample(terrainUpdateEnabled);
                }
            }

            foreach (var nullTerrainInstanceId in nullTerrainsInstanceIds)
            {
                RecordDestruction(nullTerrainInstanceId);
                RemoveFromCache(nullTerrainInstanceId);
            }
        }

        private void RecordCreation(Terrain terrain)
        {
            var terrainIdentifier = terrain.ToIdentifierPayload();

            var terrainCreate = new TerrainCreate { Id = terrainIdentifier };

            var terrainUpdate = new TerrainUpdate
            {
                Id = terrainIdentifier,
                TerrainDataId = terrain.terrainData == null ? null : terrain.terrainData.ToAssetIdentifierPayload(),
                TreeDistance = terrain.treeDistance,
                TreeBillboardDistance = terrain.treeBillboardDistance,
                TreeCrossFadeLength = terrain.treeCrossFadeLength,
                TreeMaximumFullLodCount = terrain.treeMaximumFullLODCount,
                DetailObjectDistance = terrain.detailObjectDistance,
                DetailObjectDensity = terrain.detailObjectDensity,
                HeightmapPixelError = terrain.heightmapPixelError,
                HeightmapMaximumLod = terrain.heightmapMaximumLOD,
                BasemapDistance = terrain.basemapDistance,
                LightmapIndex = terrain.lightmapIndex,
                RealtimeLightmapIndex = terrain.realtimeLightmapIndex,
                LightmapScaleOffset = terrain.lightmapScaleOffset.ToPayload(),
                RealtimeLightmapScaleOffset = terrain.realtimeLightmapScaleOffset.ToPayload(),
                KeepUnusedRenderingResources = terrain.keepUnusedRenderingResources,
                ShadowCastingMode = terrain.shadowCastingMode.ToPayload(),
                ReflectionProbeUsage = terrain.reflectionProbeUsage.ToPayload(),
                MaterialTemplateId = terrain.materialTemplate == null ? null : terrain.materialTemplate.ToAssetIdentifierPayload(),
                DrawHeightmap = terrain.drawHeightmap,
                AllowAutoConnect = terrain.allowAutoConnect,
                GroupingId = terrain.groupingID,
                DrawInstanced = terrain.drawInstanced,
                NormalmapTextureId = terrain.normalmapTexture == null ? null : terrain.normalmapTexture.ToAssetIdentifierPayload(),
                DrawTreesAndFoliage = terrain.drawTreesAndFoliage,
                PatchBoundsMultiplier = terrain.patchBoundsMultiplier.ToPayload(),
                TreeLodBiasMultiplier = terrain.treeLODBiasMultiplier,
                CollectDetailPatches = terrain.collectDetailPatches,
            };

            var terrainUpdateEnabled = new TerrainUpdateEnabled
            {
                Id = terrain.ToIdentifierPayload(),
                Enabled = terrain.enabled
            };

            recorder.RecordSample(terrainCreate);
            recorder.RecordSample(terrainUpdate);
            recorder.RecordSample(terrainUpdateEnabled);
        }

        private void RecordDestruction(int terrainInstanceId)
        {
            var terrainDestroy = new ComponentDestroy
                { Id = new ComponentDestroyIdentifier { Id = terrainInstanceId.ToString() } };
            recorder.RecordSample(terrainDestroy);
        }
    }
}