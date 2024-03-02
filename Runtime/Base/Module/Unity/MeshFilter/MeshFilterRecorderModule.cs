using System.Collections.Generic;
using PLUME.Base.Events;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Core.Utils;
using PLUME.Sample.Unity;
using UnityEngine;
using UnityEngine.Scripting;
using MeshFilterSafeRef = PLUME.Core.Object.SafeRef.IComponentSafeRef<UnityEngine.MeshFilter>;

namespace PLUME.Base.Module.Unity.MeshFilter
{
    [Preserve]
    public class MeshFilterRecorderModule : ComponentRecorderModule<UnityEngine.MeshFilter, MeshFilterFrameData>
    {
        private readonly Dictionary<MeshFilterSafeRef, MeshFilterCreate> _createSamples = new();
        private readonly Dictionary<MeshFilterSafeRef, MeshFilterDestroy> _destroySamples = new();
        private readonly Dictionary<MeshFilterSafeRef, MeshFilterUpdate> _updateSamples = new();

        protected override void OnCreate(RecorderContext ctx)
        {
            base.OnCreate(ctx);
            MeshFilterEvents.OnMeshChanged += (mf, mesh) => OnMeshChanged(mf, mesh, ctx);
            MeshFilterEvents.OnSharedMeshChanged += (mf, mesh) => OnMeshChanged(mf, mesh, ctx);
        }

        protected override void OnObjectMarkedCreated(MeshFilterSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);
            var meshAssetSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateAssetSafeRef(objSafeRef.Component.sharedMesh);

            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.MeshId = meshAssetSafeRef.ToAssetIdentifierPayload();
            _createSamples[objSafeRef] = new MeshFilterCreate { Id = objSafeRef.ToIdentifierPayload() };
        }

        protected override void OnObjectMarkedDestroyed(MeshFilterSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedDestroyed(objSafeRef, ctx);
            _destroySamples[objSafeRef] = new MeshFilterDestroy { Id = objSafeRef.ToIdentifierPayload() };
        }

        /// <summary>
        /// This method is called when the <see cref="MeshFilter.sharedMesh"/> or <see cref="MeshFilter.mesh"/> is set
        /// or when the <see cref="MeshFilter.mesh"/> property is queried, which might result in a mesh instantiation
        /// (cf. https://docs.unity3d.com/ScriptReference/MeshFilter-mesh.html).
        /// </summary>
        private void OnMeshChanged(UnityEngine.MeshFilter meshFilter, Mesh mesh, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(meshFilter);

            if (!IsRecordingObject(objSafeRef))
                return;
            
            var meshAssetSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateAssetSafeRef(mesh);
            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.MeshId = meshAssetSafeRef.ToAssetIdentifierPayload();
        }

        private MeshFilterUpdate GetOrCreateUpdateSample(MeshFilterSafeRef objSafeRef)
        {
            if (_updateSamples.TryGetValue(objSafeRef, out var sample))
                return sample;
            sample = new MeshFilterUpdate { Id = objSafeRef.ToIdentifierPayload() };
            _updateSamples[objSafeRef] = sample;
            return sample;
        }

        protected override MeshFilterFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            var frameData = MeshFilterFrameData.Pool.Get();
            frameData.AddCreateSamples(_createSamples.Values);
            frameData.AddDestroySamples(_destroySamples.Values);
            frameData.AddUpdateSamples(_updateSamples.Values);
            return frameData;
        }

        protected override void OnAfterCollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            base.OnAfterCollectFrameData(frameInfo, ctx);
            _createSamples.Clear();
            _destroySamples.Clear();
            _updateSamples.Clear();
        }
    }
}