using System.Collections.Generic;
using PLUME.Base.Hooks;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Core.Utils;
using PLUME.Sample.Unity;
using UnityEngine;
using UnityEngine.Scripting;
using MeshFilterSafeRef = PLUME.Core.Object.SafeRef.ComponentSafeRef<UnityEngine.MeshFilter>;

namespace PLUME.Base.Module.Unity.MeshFilter
{
    [Preserve]
    public class MeshFilterRecorderModule : ComponentRecorderModule<UnityEngine.MeshFilter, MeshFilterFrameData>
    {
        private readonly FrameDataPool<MeshFilterFrameData> _frameDataPool = new();

        private MeshFilterFrameData _frameData;
        
        // Keep track of the last mesh id to detect changes and create update only when necessary
        private readonly Dictionary<MeshFilterSafeRef, int> _lastMeshId = new();

        protected override void OnCreate(RecorderContext ctx)
        {
            _frameData = _frameDataPool.Get();

            ObjectHooks.OnBeforeDestroy += obj => OnBeforeDestroyObject(obj, ctx);
            GameObjectHooks.OnAddComponent += (go, component) => OnAddComponent(go, component, ctx);
            MeshFilterHooks.OnSetMesh += (mf, _) => OnMeshTouched(mf, ctx);
            MeshFilterHooks.OnSetSharedMesh += (mf, _) => OnMeshTouched(mf, ctx);
            MeshFilterHooks.OnGetMesh += (mf, _) => OnMeshTouched(mf, ctx);
        }

        protected override void OnStartRecordingObject(MeshFilterSafeRef objSafeRef, RecorderContext ctx)
        {
            _lastMeshId[objSafeRef] = objSafeRef.Component.sharedMesh.GetInstanceID();
        }

        protected override void OnStopRecordingObject(MeshFilterSafeRef objSafeRef, RecorderContext ctx)
        {
            _lastMeshId.Remove(objSafeRef);
        }
        
        protected override void OnStopRecording(RecorderContext ctx)
        {
            _lastMeshId.Clear();
        }

        private void OnBeforeDestroyObject(Object obj, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            if (obj is not UnityEngine.MeshFilter meshFilter)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(meshFilter);

            if (!IsRecordingObject(objSafeRef))
                return;

            StopRecordingObject(objSafeRef, true, ctx);
        }

        private void OnAddComponent(UnityEngine.GameObject go, Component component, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            if (component is not UnityEngine.MeshFilter meshFilter)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(meshFilter);

            if (!IsRecordingObject(objSafeRef))
                return;

            StartRecordingObject(objSafeRef, true, ctx);
        }

        protected override void OnObjectMarkedCreated(MeshFilterSafeRef objSafeRef, RecorderContext ctx)
        {
            var meshAssetSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateAssetSafeRef(objSafeRef.Component.sharedMesh);

            var updateSample = new MeshFilterUpdate
            {
                Id = objSafeRef.ToIdentifierPayload(),
                MeshId = meshAssetSafeRef.ToAssetIdentifierPayload()
            };
            
            _frameData.AddCreateSample(new MeshFilterCreate { Id = objSafeRef.ToIdentifierPayload() });
            _frameData.AddUpdateSample(updateSample);
        }

        protected override void OnObjectMarkedDestroyed(MeshFilterSafeRef objSafeRef, RecorderContext ctx)
        {
            _frameData.AddDestroySample(new MeshFilterDestroy { Id = objSafeRef.ToIdentifierPayload() });
        }

        /// <summary>
        /// This method is called when the <see cref="MeshFilter.sharedMesh"/> or <see cref="MeshFilter.mesh"/> is set
        /// or when the <see cref="MeshFilter.mesh"/> property is queried, which might result in a mesh instantiation
        /// (cf. https://docs.unity3d.com/ScriptReference/MeshFilter-mesh.html).
        /// </summary>
        /// <param name="meshFilter"></param>
        /// <param name="ctx"></param>
        private void OnMeshTouched(UnityEngine.MeshFilter meshFilter, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(meshFilter);

            if (!IsRecordingObject(objSafeRef))
                return;

            // When the mesh is instantiated (not shared with other mesh filters), the sharedMesh property points to the
            // same instance as the mesh property. So we handle both cases at once.
            var sharedMesh = objSafeRef.Component.sharedMesh;
            var meshInstanceId = sharedMesh.GetInstanceID();
            
            var meshAssetSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateAssetSafeRef(sharedMesh);

            // The mesh instance has not changed, no need to create an update sample.
            if(_lastMeshId[objSafeRef] == meshInstanceId)
                return;
            
            var updateSample = new MeshFilterUpdate
            {
                Id = objSafeRef.ToIdentifierPayload(),
                MeshId = meshAssetSafeRef.ToAssetIdentifierPayload()
            };

            _frameData.AddUpdateSample(updateSample);
            _lastMeshId[objSafeRef] = meshInstanceId;
        }

        protected override MeshFilterFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            // Collect the frame data and create a fresh instance for the next frame
            var collectedFrameData = _frameData;
            _frameData = _frameDataPool.Get();
            return collectedFrameData;
        }
    }
}