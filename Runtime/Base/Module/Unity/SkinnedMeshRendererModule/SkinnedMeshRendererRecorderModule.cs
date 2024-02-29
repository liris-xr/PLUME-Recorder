using PLUME.Base.Hooks;
using PLUME.Base.Module.Unity;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Core.Utils;
using PLUME.Sample.Unity;
using UnityEngine;
using UnityEngine.Scripting;
using Object = UnityEngine.Object;
using SkinnedMeshRendererSafeRef = PLUME.Core.Object.SafeRef.ComponentSafeRef<UnityEngine.SkinnedMeshRenderer>;

namespace PLUME
{
    [Preserve]
    public class SkinnedMeshRendererRecorderModule : ComponentRecorderModule<UnityEngine.SkinnedMeshRenderer, SkinnedMeshRendererFrameData>
    {
        private readonly FrameDataPool<SkinnedMeshRendererFrameData> _frameDataPool = new();

        private SkinnedMeshRendererFrameData _frameData;

        protected override void OnCreate(RecorderContext ctx)
        {
            _frameData = _frameDataPool.Get();

            ObjectHooks.OnBeforeDestroy += obj => OnBeforeDestroy(obj, ctx);
            GameObjectHooks.OnAddComponent += (go, component) => OnAddComponent(go, component, ctx);

            SkinnedMeshRendererHooks.OnSetMaterial += (mr, material) => OnSetMaterial(mr, material, false, ctx);
            SkinnedMeshRendererHooks.OnSetSharedMaterial += (mr, material) => OnSetMaterial(mr, material, true, ctx);
            SkinnedMeshRendererHooks.OnSetMaterials += (mr, materials) => OnSetMaterials(mr, materials, false, ctx);
            SkinnedMeshRendererHooks.OnSetSharedMaterials += (mr, materials) => OnSetMaterials(mr, materials, true, ctx);
            
            SkinnedMeshRendererHooks.OnSetBones += (mr, bones) => OnSetBones(mr, bones, ctx);
            SkinnedMeshRendererHooks.OnSetLocalBounds += (mr, localBounds) => OnSetLocalBounds(mr, localBounds, ctx);
            SkinnedMeshRendererHooks.OnSetWorldBounds += (mr, worldBounds) => OnSetWorldBounds(mr, worldBounds, ctx);
            
            SkinnedMeshRendererHooks.OnSetLightmapIndex += (mr, lightmapIndex) => OnSetLightmapIndex(mr, lightmapIndex, ctx);
            SkinnedMeshRendererHooks.OnSetLightmapScaleOffset += (mr, lightmapScaleOffset) => OnSetLightmapScaleOffset(mr, lightmapScaleOffset, ctx);
            SkinnedMeshRendererHooks.OnSetRealtimeLightmapIndex += (mr, realtimeLightmapIndex) => OnSetRealtimeLightmapIndex(mr, realtimeLightmapIndex, ctx);
            SkinnedMeshRendererHooks.OnSetRealtimeLightmapScaleOffset += (mr, realtimeLightmapScaleOffset) => OnSetRealtimeLightmapScaleOffset(mr, realtimeLightmapScaleOffset, ctx);
        }

        private void OnBeforeDestroy(Object obj, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            if (obj is not UnityEngine.SkinnedMeshRenderer skinnedMeshRenderer)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(skinnedMeshRenderer);

            if (!IsRecordingObject(objSafeRef))
                return;

            StopRecordingObject(objSafeRef, true, ctx);
        }

        private void OnAddComponent(UnityEngine.GameObject go, Component component, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            if (component is not UnityEngine.SkinnedMeshRenderer skinnedMeshRenderer)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(skinnedMeshRenderer);

            if (!IsRecordingObject(objSafeRef))
                return;

            StartRecordingObject(objSafeRef, true, ctx);
        }

        protected override void OnObjectMarkedCreated(SkinnedMeshRendererSafeRef objSafeRef, RecorderContext ctx)
        {
            Vector4 lightmapScaleOffset;
            Vector4 realtimeLightmapScaleOffset;
            
            var updateSample = new SkinnedMeshRendererUpdate()
            {
                Id = objSafeRef.ToIdentifierPayload(),
                LightmapIndex = objSafeRef.Component.lightmapIndex,
                LightmapScaleOffset = new Sample.Common.Vector4
                {
                    X = (lightmapScaleOffset = objSafeRef.Component.lightmapScaleOffset).x,
                    Y = lightmapScaleOffset.y,
                    Z = lightmapScaleOffset.z,
                    W = lightmapScaleOffset.w
                },
                RealtimeLightmapIndex = objSafeRef.Component.realtimeLightmapIndex,
                RealtimeLightmapScaleOffset = new Sample.Common.Vector4
                {
                    X = (realtimeLightmapScaleOffset = objSafeRef.Component.realtimeLightmapScaleOffset).x,
                    Y = realtimeLightmapScaleOffset.y,
                    Z = realtimeLightmapScaleOffset.z,
                    W = realtimeLightmapScaleOffset.w
                }
            };

            foreach (var material in objSafeRef.Component.sharedMaterials)
            {
                var materialAssetSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateAssetSafeRef(material);
                updateSample.Materials.Ids.Add(materialAssetSafeRef.ToAssetIdentifierPayload());
            }

            _frameData.AddCreateSample(new SkinnedMeshRendererCreate() { Id = objSafeRef.ToIdentifierPayload() });
            _frameData.AddUpdateSample(updateSample);
        }

        protected override void OnObjectMarkedDestroyed(SkinnedMeshRendererSafeRef objSafeRef, RecorderContext ctx)
        {
            _frameData.AddDestroySample(new SkinnedMeshRendererDestroy { Id = objSafeRef.ToIdentifierPayload() });
        }

        private void OnSetMaterial(UnityEngine.SkinnedMeshRenderer skinnedMeshRenderer, Material material, bool isSharedMaterial,
            RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(skinnedMeshRenderer);

            if (!IsRecordingObject(objSafeRef))
                return;

            var materialAssetSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateAssetSafeRef(material);
            var updateSample = new SkinnedMeshRendererUpdate { Id = objSafeRef.ToIdentifierPayload() };
            if (isSharedMaterial)
                Debug.Log("NotImplementedException");
                //updateSample.SharedMaterials.Ids.Add(materialAssetSafeRef.ToAssetIdentifierPayload());
            else
                updateSample.Materials.Ids.Add(materialAssetSafeRef.ToAssetIdentifierPayload());
            
            _frameData.AddUpdateSample(updateSample);
        }

        private void OnSetMaterials(UnityEngine.SkinnedMeshRenderer skinnedMeshRenderer, Material[] materials, bool isSharedMaterials,
            RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(skinnedMeshRenderer);

            if (!IsRecordingObject(objSafeRef))
                return;

            var updateSample = new SkinnedMeshRendererUpdate { Id = objSafeRef.ToIdentifierPayload() };

            foreach (var material in materials)
            {
                var materialAssetSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateAssetSafeRef(material);

                if (isSharedMaterials)
                    Debug.Log("NotImplementedException");
                    //updateSample.SharedMaterials.Ids.Add(materialAssetSafeRef.ToAssetIdentifierPayload());
                else
                    updateSample.Materials.Ids.Add(materialAssetSafeRef.ToAssetIdentifierPayload());
                
                _frameData.AddUpdateSample(updateSample);
            }
        }
        
        private void OnSetBones(SkinnedMeshRenderer skinnedMeshRenderer, Transform[] bones, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(skinnedMeshRenderer);

            if (!IsRecordingObject(objSafeRef))
                return;

            var updateSample = new SkinnedMeshRendererUpdate
            {
                Id = objSafeRef.ToIdentifierPayload()
            };

            foreach (var bone in bones)
            {
                var boneSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(bone);
                updateSample.Bones.BonesIds.Add(boneSafeRef.ToIdentifierPayload());
            }
            
            _frameData.AddUpdateSample(updateSample);
        }
        
        private void OnSetLocalBounds(SkinnedMeshRenderer skinnedMeshRenderer, Bounds localBounds, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(skinnedMeshRenderer);

            if (!IsRecordingObject(objSafeRef))
                return;

            var updateSample = new SkinnedMeshRendererUpdate
            {
                Id = objSafeRef.ToIdentifierPayload(),
                LocalBounds = new Sample.Common.Bounds
                {
                    Center = new Sample.Common.Vector3
                    {
                        X = localBounds.center.x,
                        Y = localBounds.center.y,
                        Z = localBounds.center.z
                    },
                    Extents = new Sample.Common.Vector3
                    {
                        X = localBounds.size.x,
                        Y = localBounds.size.y,
                        Z = localBounds.size.z
                    }
                }
            };
            
            _frameData.AddUpdateSample(updateSample);
        }
        
        private void OnSetWorldBounds(SkinnedMeshRenderer skinnedMeshRenderer, Bounds worldBounds, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(skinnedMeshRenderer);

            if (!IsRecordingObject(objSafeRef))
                return;

            var updateSample = new SkinnedMeshRendererUpdate
            {
                Id = objSafeRef.ToIdentifierPayload(),
                WorldBounds = new Sample.Common.Bounds
                {
                    Center = new Sample.Common.Vector3
                    {
                        X = worldBounds.center.x,
                        Y = worldBounds.center.y,
                        Z = worldBounds.center.z
                    },
                    Extents = new Sample.Common.Vector3
                    {
                        X = worldBounds.size.x,
                        Y = worldBounds.size.y,
                        Z = worldBounds.size.z
                    }
                }
            };
            
            _frameData.AddUpdateSample(updateSample);
        }

        private void OnSetLightmapIndex(SkinnedMeshRenderer skinnedMeshRenderer, int lightmapIndex, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(skinnedMeshRenderer);

            if (!IsRecordingObject(objSafeRef))
                return;

            var updateSample = new SkinnedMeshRendererUpdate
            {
                Id = objSafeRef.ToIdentifierPayload(),
                LightmapIndex = lightmapIndex
            };
            
            _frameData.AddUpdateSample(updateSample);
        }

        private void OnSetLightmapScaleOffset(SkinnedMeshRenderer skinnedMeshRenderer, Vector4 lightmapScaleOffset,
            RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(skinnedMeshRenderer);

            if (!IsRecordingObject(objSafeRef))
                return;

            var updateSample = new SkinnedMeshRendererUpdate
            {
                Id = objSafeRef.ToIdentifierPayload(),
                LightmapScaleOffset = new Sample.Common.Vector4
                {
                    X = lightmapScaleOffset.x,
                    Y = lightmapScaleOffset.y,
                    Z = lightmapScaleOffset.z,
                    W = lightmapScaleOffset.w
                }
            };
            
            _frameData.AddUpdateSample(updateSample);
        }

        private void OnSetRealtimeLightmapIndex(SkinnedMeshRenderer skinnedMeshRenderer, int realtimeLightmapIndex,
            RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(skinnedMeshRenderer);

            if (!IsRecordingObject(objSafeRef))
                return;

            var updateSample = new SkinnedMeshRendererUpdate
            {
                Id = objSafeRef.ToIdentifierPayload(),
                LightmapIndex = realtimeLightmapIndex
            };
            
            _frameData.AddUpdateSample(updateSample);
        }

        private void OnSetRealtimeLightmapScaleOffset(SkinnedMeshRenderer skinnedMeshRenderer, Vector4 realtimeLightmapScaleOffset,
            RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(skinnedMeshRenderer);

            if (!IsRecordingObject(objSafeRef))
                return;

            var updateSample = new SkinnedMeshRendererUpdate
            {
                Id = objSafeRef.ToIdentifierPayload(),
                LightmapScaleOffset = new Sample.Common.Vector4
                {
                    X = realtimeLightmapScaleOffset.x,
                    Y = realtimeLightmapScaleOffset.y,
                    Z = realtimeLightmapScaleOffset.z,
                    W = realtimeLightmapScaleOffset.w
                }
            };
            
            _frameData.AddUpdateSample(updateSample);
        }


        protected override SkinnedMeshRendererFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            var collectedFrameData = _frameData;
            _frameData = _frameDataPool.Get();
            return collectedFrameData;
        }
    }
}