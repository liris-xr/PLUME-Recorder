using System.Collections.Generic;
using System.Linq;
using PLUME.Base.Hooks;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Core.Utils;
using PLUME.Sample.Unity;
using UnityEngine;

namespace PLUME.Base.Module.Unity.Renderer
{
    public abstract class RendererRecorderModule<TR, TF> : ComponentRecorderModule<TR, TF>
        where TR : UnityEngine.Renderer where TF : IFrameData
    {
        // Keep track of the last properties to detect changes and send change notifications only when necessary
        private readonly Dictionary<IComponentSafeRef<TR>, bool> _enabled = new();
        private readonly Dictionary<IComponentSafeRef<TR>, List<IAssetSafeRef<Material>>> _materials = new();
        private readonly Dictionary<IComponentSafeRef<TR>, Bounds> _localBounds = new();
        private readonly Dictionary<IComponentSafeRef<TR>, int> _lightmapIndex = new();
        private readonly Dictionary<IComponentSafeRef<TR>, Vector4> _lightmapScaleOffset = new();
        private readonly Dictionary<IComponentSafeRef<TR>, int> _realtimeLightmapIndex = new();
        private readonly Dictionary<IComponentSafeRef<TR>, Vector4> _realtimeLightmapScaleOffset = new();

        // Update samples for the current frame and for each component, entries are only added when a property changes
        private readonly Dictionary<IComponentSafeRef<TR>, RendererUpdate> _updateSamples = new();

        protected override void OnCreate(RecorderContext ctx)
        {
            base.OnCreate(ctx);

            RendererHooks.OnSetEnabled += (r, _) => OnEnabledUpdate(r, ctx);
            RendererHooks.OnSetMaterial += (r, _) => OnMaterialsUpdate(r, ctx);
            RendererHooks.OnSetSharedMaterial += (r, _) => OnMaterialsUpdate(r, ctx);
            RendererHooks.OnSetMaterials += (r, _) => OnMaterialsUpdate(r, ctx);
            RendererHooks.OnSetSharedMaterials += (r, _) => OnMaterialsUpdate(r, ctx);
            RendererHooks.OnSetLocalBounds += (r, _) => OnLocalBoundsUpdate(r, ctx);
            RendererHooks.OnResetLocalBounds += (r) => OnLocalBoundsUpdate(r, ctx);
            RendererHooks.OnSetLightmapIndex += (r, _) => OnLightmapIndexUpdate(r, ctx);
            RendererHooks.OnSetLightmapScaleOffset += (r, _) => OnLightmapScaleOffsetUpdate(r, ctx);
            RendererHooks.OnSetRealtimeLightmapIndex += (r, _) => OnRealtimeLightmapIndexUpdate(r, ctx);
            RendererHooks.OnSetRealtimeLightmapScaleOffset += (r, _) => OnRealtimeLightmapScaleOffsetUpdate(r, ctx);
        }

        protected override void OnStartRecordingObject(IComponentSafeRef<TR> objSafeRef, RecorderContext ctx)
        {
            base.OnStartRecordingObject(objSafeRef, ctx);
            
            _enabled[objSafeRef] = objSafeRef.Component.enabled;
            _materials[objSafeRef] = objSafeRef.Component.sharedMaterials
                .Select(m => ctx.ObjectSafeRefProvider.GetOrCreateAssetSafeRef(m))
                .ToList();
            _localBounds[objSafeRef] = objSafeRef.Component.localBounds;
            _lightmapIndex[objSafeRef] = objSafeRef.Component.lightmapIndex;
            _lightmapScaleOffset[objSafeRef] = objSafeRef.Component.lightmapScaleOffset;
            _realtimeLightmapIndex[objSafeRef] = objSafeRef.Component.realtimeLightmapIndex;
            _realtimeLightmapScaleOffset[objSafeRef] = objSafeRef.Component.realtimeLightmapScaleOffset;
        }

        protected override void OnStopRecordingObject(IComponentSafeRef<TR> objSafeRef, RecorderContext ctx)
        {
            base.OnStopRecordingObject(objSafeRef, ctx);
            
            _enabled.Remove(objSafeRef);
            _materials.Remove(objSafeRef);
            _localBounds.Remove(objSafeRef);
            _lightmapIndex.Remove(objSafeRef);
            _lightmapScaleOffset.Remove(objSafeRef);
            _realtimeLightmapIndex.Remove(objSafeRef);
            _realtimeLightmapScaleOffset.Remove(objSafeRef);
        }

        protected override void OnStopRecording(RecorderContext ctx)
        {
            base.OnStopRecording(ctx);
            
            _enabled.Clear();
            _materials.Clear();
            _localBounds.Clear();
            _lightmapIndex.Clear();
            _lightmapScaleOffset.Clear();
            _realtimeLightmapIndex.Clear();
            _realtimeLightmapScaleOffset.Clear();
        }

        protected override void OnObjectMarkedCreated(IComponentSafeRef<TR> objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);
            
            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.Enabled = objSafeRef.Component.enabled;
            updateSample.Materials = new RendererUpdate.Types.Materials();
            updateSample.LocalBounds = objSafeRef.Component.localBounds.ToPayload();
            updateSample.LightmapIndex = objSafeRef.Component.lightmapIndex;
            updateSample.LightmapScaleOffset = objSafeRef.Component.lightmapScaleOffset.ToPayload();
            updateSample.RealtimeLightmapIndex = objSafeRef.Component.realtimeLightmapIndex;
            updateSample.RealtimeLightmapScaleOffset = objSafeRef.Component.realtimeLightmapScaleOffset.ToPayload();
            
            updateSample.Materials.Ids.AddRange(objSafeRef.Component.sharedMaterials.Select(m =>
                ctx.ObjectSafeRefProvider.GetOrCreateAssetSafeRef(m).ToAssetIdentifierPayload()));
        }
        
        protected IEnumerable<RendererUpdate> GetRendererUpdateSamples()
        {
            return _updateSamples.Values;
        }
        
        protected override void OnAfterCollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            base.OnAfterCollectFrameData(frameInfo, ctx);
            _updateSamples.Clear();
        }

        private RendererUpdate GetOrCreateUpdateSample(IComponentSafeRef<TR> objSafeRef)
        {
            if (_updateSamples.TryGetValue(objSafeRef, out var sample))
                return sample;
            _updateSamples[objSafeRef] = new RendererUpdate { Id = objSafeRef.ToIdentifierPayload() };
            return _updateSamples[objSafeRef];
        }

        private void OnEnabledUpdate(UnityEngine.Renderer renderer, RecorderContext ctx)
        {
            if (renderer is not TR typedRenderer)
                return;

            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(typedRenderer);

            if (!IsRecordingObject(objSafeRef))
                return;

            var enabled = objSafeRef.Component.enabled;

            if (_enabled[objSafeRef] == enabled)
                return;

            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.Enabled = enabled;

            _enabled[objSafeRef] = enabled;
        }

        /// <summary>
        /// This method is called when the <see cref="SkinnedMeshRenderer.material"/> or <see cref="SkinnedMeshRenderer.material"/> is set
        /// or when the <see cref="SkinnedMeshRenderer.material"/> property is queried, which might result in a mesh instantiation
        /// (cf. https://docs.unity3d.com/ScriptReference/MeshFilter-mesh.html).
        /// </summary>
        private void OnMaterialsUpdate(UnityEngine.Renderer renderer, RecorderContext ctx)
        {
            if (renderer is not TR typedRenderer)
                return;

            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(typedRenderer);

            if (!IsRecordingObject(objSafeRef))
                return;

            // When the material is instantiated (not shared with other renderers), the sharedMaterial property points to the
            // same instance as the material property. So we handle both cases at once.
            var materials = objSafeRef.Component.sharedMaterials;
            var materialsAssetSafeRefs = materials
                .Select(m => ctx.ObjectSafeRefProvider.GetOrCreateAssetSafeRef(m))
                .ToList();

            // The mesh instance has not changed, no need to create an update sample.
            if (_materials[objSafeRef].SequenceEqual(materialsAssetSafeRefs))
                return;

            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.Materials = new RendererUpdate.Types.Materials();
            updateSample.Materials.Ids.AddRange(materialsAssetSafeRefs.Select(m => m.ToAssetIdentifierPayload()));

            _materials[objSafeRef] = materialsAssetSafeRefs;
        }

        private void OnLocalBoundsUpdate(UnityEngine.Renderer renderer, RecorderContext ctx)
        {
            if (renderer is not TR typedRenderer)
                return;

            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(typedRenderer);

            if (!IsRecordingObject(objSafeRef))
                return;

            var localBounds = objSafeRef.Component.localBounds;

            if (_localBounds[objSafeRef].Equals(localBounds))
                return;
            
            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.LocalBounds = localBounds.ToPayload();
            
            _localBounds[objSafeRef] = localBounds;
        }

        private void OnLightmapIndexUpdate(UnityEngine.Renderer renderer, RecorderContext ctx)
        {
            if (renderer is not TR typedRenderer)
                return;

            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(typedRenderer);

            if (!IsRecordingObject(objSafeRef))
                return;

            var lightmapIndex = objSafeRef.Component.lightmapIndex;

            if (_lightmapIndex[objSafeRef] == lightmapIndex)
                return;

            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.LightmapIndex = lightmapIndex;

            _lightmapIndex[objSafeRef] = lightmapIndex;
        }

        private void OnLightmapScaleOffsetUpdate(UnityEngine.Renderer renderer, RecorderContext ctx)
        {
            if (renderer is not TR typedRenderer)
                return;

            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(typedRenderer);

            if (!IsRecordingObject(objSafeRef))
                return;

            var lightmapScaleOffset = objSafeRef.Component.lightmapScaleOffset;

            if (_lightmapScaleOffset[objSafeRef] == lightmapScaleOffset)
                return;
            
            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.LightmapScaleOffset = lightmapScaleOffset.ToPayload();
            
            _lightmapScaleOffset[objSafeRef] = lightmapScaleOffset;
        }

        private void OnRealtimeLightmapIndexUpdate(UnityEngine.Renderer renderer, RecorderContext ctx)
        {
            if (renderer is not TR typedRenderer)
                return;

            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(typedRenderer);

            if (!IsRecordingObject(objSafeRef))
                return;

            var realtimeLightmapIndex = objSafeRef.Component.realtimeLightmapIndex;

            if (_realtimeLightmapIndex[objSafeRef] == realtimeLightmapIndex)
                return;

            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.RealtimeLightmapIndex = realtimeLightmapIndex;

            _realtimeLightmapIndex[objSafeRef] = realtimeLightmapIndex;
        }

        private void OnRealtimeLightmapScaleOffsetUpdate(UnityEngine.Renderer renderer, RecorderContext ctx)
        {
            if (renderer is not TR typedRenderer)
                return;

            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(typedRenderer);

            if (!IsRecordingObject(objSafeRef))
                return;

            var realtimeLightmapScaleOffset = objSafeRef.Component.realtimeLightmapScaleOffset;

            if (_realtimeLightmapScaleOffset[objSafeRef] == realtimeLightmapScaleOffset)
                return;

            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.RealtimeLightmapScaleOffset = realtimeLightmapScaleOffset.ToPayload();

            _realtimeLightmapScaleOffset[objSafeRef] = realtimeLightmapScaleOffset;
        }
    }
}