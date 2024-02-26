using PLUME.Base.Hooks;
using PLUME.Base.Module.Unity.MeshFilter.Sample;
using PLUME.Core.Object;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.ProtoBurst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace PLUME.Base.Module.Unity.MeshFilter
{
    public class MeshFilterRecorderModule :
        ObjectFrameDataRecorderModuleBase<UnityEngine.MeshFilter, MeshFilterFrameData>
    {
        private NativeHashMap<ObjectIdentifier, MeshFilterState> _states;
        private NativeHashMap<ObjectIdentifier, ObjectIdentifier> _parentIdentifiers;

        protected override void OnCreate(RecorderContext ctx)
        {
            _states = new NativeHashMap<ObjectIdentifier, MeshFilterState>(100, Allocator.Persistent);
            _parentIdentifiers = new NativeHashMap<ObjectIdentifier, ObjectIdentifier>(100, Allocator.Persistent);

            MeshFilterHooks.OnSetMesh += (mf, mesh) => OnSetMesh(mf, mesh, ctx);
            MeshFilterHooks.OnSetSharedMesh += (mf, mesh) => OnSetSharedMesh(mf, mesh, ctx);
        }

        protected override void OnDestroy(RecorderContext ctx)
        {
            _states.Dispose();
            _parentIdentifiers.Dispose();
        }

        protected override void OnStartRecordingObject(ObjectSafeRef<UnityEngine.MeshFilter> objSafeRef, Record record,
            RecorderContext ctx)
        {
            var meshFilter = objSafeRef.TypedObject;
            var goSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateTypedObjectSafeRef(meshFilter.gameObject);
            
            var initialState = new MeshFilterState
            {
                MeshIdentifier = GetMeshAssetIdentifier(meshFilter.mesh, ctx),
                SharedMeshIdentifier = GetMeshAssetIdentifier(meshFilter.sharedMesh, ctx)
            };
            
            _states[objSafeRef.Identifier] = initialState;
            _parentIdentifiers[objSafeRef.Identifier] = goSafeRef.Identifier;
        }

        protected override void OnStopRecordingObject(ObjectSafeRef<UnityEngine.MeshFilter> objSafeRef, Record record,
            RecorderContext recorderContext)
        {
            _states.Remove(objSafeRef.Identifier);
            _parentIdentifiers.Remove(objSafeRef.Identifier);
        }

        protected override void OnStopRecording(Record record, RecorderContext recorderContext)
        {
            _states.Clear();
            _parentIdentifiers.Clear();
        }

        private void OnSetMesh(UnityEngine.MeshFilter meshFilter, Mesh mesh, RecorderContext ctx)
        {
            if (!IsRecording)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateTypedObjectSafeRef(meshFilter);
            
            if (!IsRecordingObject(objSafeRef))
                return;
            
            var meshIdentifier = GetMeshAssetIdentifier(mesh, ctx);

            var state = _states[objSafeRef.Identifier];
            state.MeshIdentifierDirty = meshIdentifier != state.MeshIdentifier;
            state.MeshIdentifier = meshIdentifier;
        }

        private void OnSetSharedMesh(UnityEngine.MeshFilter meshFilter, Mesh sharedMesh, RecorderContext ctx)
        {
            if (!IsRecording)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateTypedObjectSafeRef(meshFilter);
            
            if (!IsRecordingObject(objSafeRef))
                return;
            
            var sharedMeshIdentifier = GetMeshAssetIdentifier(sharedMesh, ctx);

            var state = _states[objSafeRef.Identifier];
            state.SharedMeshIdentifierDirty = sharedMeshIdentifier != state.MeshIdentifier;
            state.SharedMeshIdentifier = sharedMeshIdentifier;
        }

        protected override MeshFilterFrameData CollectFrameData(FrameInfo frameInfo, Record record,
            RecorderContext context)
        {
            var updateSamples = new NativeList<MeshFilterUpdate>(RecordedObjects.Count, Allocator.Persistent);
            var createSamples = new NativeList<MeshFilterCreate>(CreatedObjectsIdentifier.Count, Allocator.Persistent);
            var destroySamples =
                new NativeList<MeshFilterDestroy>(DestroyedObjectsIdentifier.Count, Allocator.Persistent);

            var recordedObjectsIdentifier = RecordedObjectsIdentifier.ToNativeArray(Allocator.Persistent);

            new SampleProducerJob
            {
                Identifiers = recordedObjectsIdentifier,
                ParentIdentifiers = _parentIdentifiers, 
                CreatedInFrameIdentifiers = CreatedObjectsIdentifier,
                DestroyedInFrameIdentifiers = DestroyedObjectsIdentifier,
                States = _states,
                UpdateSamples = updateSamples.AsParallelWriter(),
                CreateSamples = createSamples.AsParallelWriter(),
                DestroySamples = destroySamples.AsParallelWriter()
            }.RunBatch(RecordedObjects.Count);

            recordedObjectsIdentifier.Dispose();

            return new MeshFilterFrameData(updateSamples, createSamples, destroySamples);
        }
        
        private static AssetIdentifier GetMeshAssetIdentifier(Mesh mesh, RecorderContext ctx)
        {
            var identifier = AssetIdentifier.Null;
            
            if (mesh != null)
            {
                var meshSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateTypedObjectSafeRef(mesh);

                if (meshSafeRef is AssetObjectSafeRef<Mesh> sharedMeshAssetSafeRef)
                {
                    identifier = new AssetIdentifier(sharedMeshAssetSafeRef.Identifier, sharedMeshAssetSafeRef.AssetPath);
                }
                else if (meshSafeRef is SceneObjectSafeRef<Mesh> sharedMeshSceneSafeRef)
                {
                    identifier = new AssetIdentifier(sharedMeshSceneSafeRef.Identifier, "");
                }
            }
            
            return identifier;
        }
    }
}