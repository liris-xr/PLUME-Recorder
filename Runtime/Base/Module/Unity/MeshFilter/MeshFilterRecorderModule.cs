using PLUME.Base.Hooks;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Core.Utils;
using PLUME.Sample.Unity;
using UnityEngine;
using UnityEngine.Scripting;

namespace PLUME.Base.Module.Unity.MeshFilter
{
    [Preserve]
    public class MeshFilterRecorderModule : ComponentRecorderModule<UnityEngine.MeshFilter, MeshFilterFrameData>
    {
        private readonly FrameDataPool<MeshFilterFrameData> _frameDataPool = new();
        
        private MeshFilterFrameData _frameData;

        protected override void OnCreate(RecorderContext ctx)
        {
            _frameData = _frameDataPool.Get();
            
            ObjectHooks.OnBeforeDestroy += obj => OnBeforeDestroy(obj, ctx);
            GameObjectHooks.OnAddComponent += (go, component) => OnAddComponent(go, component, ctx);
            MeshFilterHooks.OnSetMesh += (mf, mesh) => OnSetMesh(mf, mesh, false, ctx);
            MeshFilterHooks.OnSetSharedMesh += (mf, mesh) => OnSetMesh(mf, mesh, true, ctx);
        }
        
        private void OnBeforeDestroy(Object obj, RecorderContext ctx)
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

        protected override void OnObjectMarkedCreated(ComponentSafeRef<UnityEngine.MeshFilter> objSafeRef)
        {
            _frameData.AddCreateSample(new MeshFilterCreate { Id = objSafeRef.ToIdentifierPayload() });
        }

        protected override void OnObjectMarkedDestroyed(ComponentSafeRef<UnityEngine.MeshFilter> objSafeRef)
        {
            _frameData.AddDestroySample(new MeshFilterDestroy { Id = objSafeRef.ToIdentifierPayload() });
        }
        
        private void OnSetMesh(UnityEngine.MeshFilter meshFilter, Mesh mesh, bool isSharedMesh, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(meshFilter);

            if (!IsRecordingObject(objSafeRef))
                return;

            var meshAssetSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateAssetSafeRef(mesh);
            var updateSample = new MeshFilterUpdate { Id = objSafeRef.ToIdentifierPayload() };

            if (isSharedMesh)
                updateSample.SharedMeshId = meshAssetSafeRef.ToAssetIdentifierPayload();
            else
                updateSample.MeshId = meshAssetSafeRef.ToAssetIdentifierPayload();

            _frameData.AddUpdateSample(updateSample);
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