using PLUME.Base.Hooks;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Core.Utils;
using PLUME.Sample.Unity;
using UnityEngine;
using UnityEngine.Scripting;
using MeshRendererSafeRef = PLUME.Core.Object.SafeRef.ComponentSafeRef<UnityEngine.MeshRenderer>;

namespace PLUME.Base.Module.Unity.MeshRenderer
{
    [Preserve]
    public class MeshRendererRecorderModule : ComponentRecorderModule<UnityEngine.MeshRenderer, MeshRendererFrameData>
    {
        private readonly FrameDataPool<MeshRendererFrameData> _frameDataPool = new();

        private MeshRendererFrameData _frameData;

        protected override void OnCreate(RecorderContext ctx)
        {
            _frameData = _frameDataPool.Get();

            ObjectHooks.OnBeforeDestroy += obj => OnBeforeDestroy(obj, ctx);
            GameObjectHooks.OnAddComponent += (go, component) => OnAddComponent(go, component, ctx);

            MeshRendererHooks.OnSetMaterial += (mr, material) => OnSetMaterial(mr, material, false, ctx);
            MeshRendererHooks.OnSetSharedMaterial += (mr, material) => OnSetMaterial(mr, material, true, ctx);
            MeshRendererHooks.OnSetMaterials += (mr, materials) => OnSetMaterials(mr, materials, false, ctx);
            MeshRendererHooks.OnSetSharedMaterials += (mr, materials) => OnSetMaterials(mr, materials, true, ctx);

            MeshRendererHooks.OnSetLightmapIndex += (mr, lightmapIndex) => OnSetLightmapIndex(mr, lightmapIndex, ctx);
            MeshRendererHooks.OnSetLightmapScaleOffset += (mr, lightmapScaleOffset) =>
                OnSetLightmapScaleOffset(mr, lightmapScaleOffset, ctx);
            MeshRendererHooks.OnSetRealtimeLightmapIndex += (mr, realtimeLightmapIndex) =>
                OnSetRealtimeLightmapIndex(mr, realtimeLightmapIndex, ctx);
            MeshRendererHooks.OnSetRealtimeLightmapScaleOffset += (mr, realtimeLightmapScaleOffset) =>
                OnSetRealtimeLightmapScaleOffset(mr, realtimeLightmapScaleOffset, ctx);
        }

        private void OnBeforeDestroy(Object obj, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            if (obj is not UnityEngine.MeshRenderer meshRenderer)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(meshRenderer);

            if (!IsRecordingObject(objSafeRef))
                return;

            StopRecordingObject(objSafeRef, true, ctx);
        }

        private void OnAddComponent(UnityEngine.GameObject go, Component component, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            if (component is not UnityEngine.MeshRenderer meshRenderer)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(meshRenderer);

            if (!IsRecordingObject(objSafeRef))
                return;

            StartRecordingObject(objSafeRef, true, ctx);
        }

        protected override void OnObjectMarkedCreated(MeshRendererSafeRef objSafeRef, RecorderContext ctx)
        {
            Vector4 lightmapScaleOffset;
            Vector4 realtimeLightmapScaleOffset;
            
            var updateSample = new MeshRendererUpdate()
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
                updateSample.SharedMaterials.Ids.Add(materialAssetSafeRef.ToAssetIdentifierPayload());
            }

            _frameData.AddCreateSample(new MeshRendererCreate() { Id = objSafeRef.ToIdentifierPayload() });
            _frameData.AddUpdateSample(updateSample);
        }

        protected override void OnObjectMarkedDestroyed(MeshRendererSafeRef objSafeRef, RecorderContext ctx)
        {
            _frameData.AddDestroySample(new MeshRendererDestroy { Id = objSafeRef.ToIdentifierPayload() });
        }

        private void OnSetMaterial(UnityEngine.MeshRenderer meshRenderer, Material material, bool isSharedMaterial,
            RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(meshRenderer);

            if (!IsRecordingObject(objSafeRef))
                return;

            var materialAssetSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateAssetSafeRef(material);
            var updateSample = new MeshRendererUpdate { Id = objSafeRef.ToIdentifierPayload() };
            if (isSharedMaterial)
                updateSample.SharedMaterials.Ids.Add(materialAssetSafeRef.ToAssetIdentifierPayload());
            else
                updateSample.Materials.Ids.Add(materialAssetSafeRef.ToAssetIdentifierPayload());

            _frameData.AddUpdateSample(updateSample);
        }

        private void OnSetMaterials(UnityEngine.MeshRenderer meshRenderer, Material[] materials, bool isSharedMaterials,
            RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(meshRenderer);

            if (!IsRecordingObject(objSafeRef))
                return;

            var updateSample = new MeshRendererUpdate { Id = objSafeRef.ToIdentifierPayload() };

            foreach (var material in materials)
            {
                var materialAssetSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateAssetSafeRef(material);

                if (isSharedMaterials)
                    updateSample.SharedMaterials.Ids.Add(materialAssetSafeRef.ToAssetIdentifierPayload());
                else
                    updateSample.Materials.Ids.Add(materialAssetSafeRef.ToAssetIdentifierPayload());
            }

            _frameData.AddUpdateSample(updateSample);
        }

        private void OnSetLightmapIndex(UnityEngine.MeshRenderer meshRenderer, int lightmapIndex, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(meshRenderer);

            if (!IsRecordingObject(objSafeRef))
                return;

            var updateSample = new MeshRendererUpdate
            {
                Id = objSafeRef.ToIdentifierPayload(),
                LightmapIndex = lightmapIndex
            };

            _frameData.AddUpdateSample(updateSample);
        }

        private void OnSetLightmapScaleOffset(UnityEngine.MeshRenderer meshRenderer, Vector4 lightmapScaleOffset,
            RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(meshRenderer);

            if (!IsRecordingObject(objSafeRef))
                return;

            var updateSample = new MeshRendererUpdate
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

        private void OnSetRealtimeLightmapIndex(UnityEngine.MeshRenderer meshRenderer, int realtimeLightmapIndex,
            RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(meshRenderer);

            if (!IsRecordingObject(objSafeRef))
                return;

            var updateSample = new MeshRendererUpdate
            {
                Id = objSafeRef.ToIdentifierPayload(),
                LightmapIndex = realtimeLightmapIndex
            };

            _frameData.AddUpdateSample(updateSample);
        }

        private void OnSetRealtimeLightmapScaleOffset(UnityEngine.MeshRenderer meshRenderer, Vector4 realtimeLightmapScaleOffset,
            RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(meshRenderer);

            if (!IsRecordingObject(objSafeRef))
                return;

            var updateSample = new MeshRendererUpdate
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


        protected override MeshRendererFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            var collectedFrameData = _frameData;
            _frameData = _frameDataPool.Get();
            return collectedFrameData;
        }
    }
}