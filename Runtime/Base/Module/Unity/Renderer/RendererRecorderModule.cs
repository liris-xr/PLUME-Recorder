using System.Collections.Generic;
using System.Linq;
using PLUME.Base.Hooks;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity;
using UnityEngine;
using static PLUME.Core.Utils.SampleUtils;

namespace PLUME.Base.Module.Unity.Renderer
{
    public abstract class RendererRecorderModule<TR, TF> : ComponentRecorderModule<TR, TF>
        where TR : UnityEngine.Renderer where TF : IFrameData
    {
        // Update samples for the current frame and for each component, entries are only added when a property changes
        private readonly Dictionary<IComponentSafeRef<TR>, RendererUpdate> _updateSamples = new();

        protected override void OnCreate(RecorderContext ctx)
        {
            base.OnCreate(ctx);

            RendererHooks.OnEnabledChanged += (r, _) => OnEnabledUpdate(r, ctx);
            RendererHooks.OnMaterialsChanged += (r, materials) => OnMaterialsChanged(r, materials, ctx);
            RendererHooks.OnSharedMaterialsChanged += (r, materials) => OnMaterialsChanged(r, materials, ctx);
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
                GetAssetIdentifierPayload(ctx.ObjectSafeRefProvider.GetOrCreateAssetSafeRef(m))));
        }

        protected IEnumerable<RendererUpdate> GetRendererUpdateSamples()
        {
            return _updateSamples.Values;
        }

        protected override void AfterCollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            base.AfterCollectFrameData(frameInfo, ctx);
            _updateSamples.Clear();
        }

        private RendererUpdate GetOrCreateUpdateSample(IComponentSafeRef<TR> objSafeRef)
        {
            if (_updateSamples.TryGetValue(objSafeRef, out var sample))
                return sample;
            _updateSamples[objSafeRef] = new RendererUpdate { Id = GetComponentIdentifierPayload(objSafeRef) };
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
            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.Enabled = enabled;
        }

        /// <summary>
        /// This method is called when the <see cref="SkinnedMeshRenderer.materials"/> or <see cref="SkinnedMeshRenderer.sharedMaterials"/> is set
        /// or when the <see cref="SkinnedMeshRenderer.materials"/> property is queried, which might result in a mesh instantiation
        /// (cf. https://docs.unity3d.com/ScriptReference/MeshFilter-mesh.html).
        /// </summary>
        private void OnMaterialsChanged(UnityEngine.Renderer renderer, IEnumerable<Material> materials,
            RecorderContext ctx)
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
            var materialsAssetSafeRefs = materials
                .Select(m => ctx.ObjectSafeRefProvider.GetOrCreateAssetSafeRef(m))
                .ToList();

            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.Materials = new RendererUpdate.Types.Materials();
            updateSample.Materials.Ids.AddRange(materialsAssetSafeRefs.Select(GetAssetIdentifierPayload));
        }
    }
}