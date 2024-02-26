using PLUME.Base.Hooks;
using PLUME.Base.Module.Unity.Transform.Job;
using PLUME.Base.Module.Unity.Transform.Sample;
using PLUME.Base.Module.Unity.Transform.State;
using PLUME.Base.Settings;
using PLUME.Core.Object;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.ProtoBurst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Scripting;

namespace PLUME.Base.Module.Unity.Transform
{
    [Preserve]
    internal class
        TransformRecorderModule : ObjectFrameDataRecorderModuleBase<UnityEngine.Transform, TransformFrameData>
    {
        private DynamicTransformAccessArray _transformAccessArray;

        private NativeHashMap<ObjectIdentifier, ObjectIdentifier> _gameObjectIdentifiers;
        private NativeHashMap<ObjectIdentifier, PositionState> _positionStates;
        private NativeHashMap<ObjectIdentifier, HierarchyState> _hierarchyStates;

        private TransformPositionStateUpdater _transformPositionStateUpdater;

        protected override void OnCreate(RecorderContext ctx)
        {
            var settings = TransformRecorderModuleSettings.GetOrCreate();
            var angularThreshold = settings.AngularThreshold;
            var positionThresholdSq = settings.PositionThreshold * settings.PositionThreshold;
            var scaleThresholdSq = settings.ScaleThreshold * settings.ScaleThreshold;

            _transformAccessArray = new DynamicTransformAccessArray();

            _positionStates = new NativeHashMap<ObjectIdentifier, PositionState>(1000, Allocator.Persistent);
            _gameObjectIdentifiers = new NativeHashMap<ObjectIdentifier, ObjectIdentifier>(1000, Allocator.Persistent);
            _hierarchyStates = new NativeHashMap<ObjectIdentifier, HierarchyState>(1000, Allocator.Persistent);

            _transformPositionStateUpdater = new TransformPositionStateUpdater(_positionStates, _transformAccessArray,
                angularThreshold, positionThresholdSq, scaleThresholdSq);

            TransformHooks.OnSetParent += (t, parent) => OnSetParent(t, parent, ctx);
        }

        protected override void OnDestroy(RecorderContext ctx)
        {
            _transformPositionStateUpdater.Dispose();
            _transformAccessArray.Dispose();
            _positionStates.Dispose();
            _gameObjectIdentifiers.Dispose();
            _hierarchyStates.Dispose();
        }

        protected override void OnStartRecordingObject(ObjectSafeRef<UnityEngine.Transform> objSafeRef, Record record,
            RecorderContext ctx)
        {
            var t = objSafeRef.TypedObject;
            var goSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateTypedObjectSafeRef(t.gameObject);

            var siblingIndex = t.GetSiblingIndex();
            var localScale = t.localScale;
            t.GetLocalPositionAndRotation(out var localPosition, out var localRotation);

            var parentIdentifier = TransformGameObjectIdentifier.Null;

            if (t.parent != null)
            {
                var parentTransformSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateTypedObjectSafeRef(t.parent);
                var parentGoSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateTypedObjectSafeRef(t.gameObject);
                parentIdentifier =
                    new TransformGameObjectIdentifier(parentTransformSafeRef.Identifier, parentGoSafeRef.Identifier);
            }

            var initialPositionState = new PositionState
            {
                LocalPosition = localPosition,
                LocalRotation = localRotation,
                LocalScale = localScale
            };
            
            var initialHierarchyState = new HierarchyState
            {
                ParentIdentifier = parentIdentifier,
                SiblingIndex = siblingIndex
            };
            
            _transformAccessArray.Add(objSafeRef);
            _gameObjectIdentifiers.Add(objSafeRef.Identifier, goSafeRef.Identifier);
            _positionStates.Add(objSafeRef.Identifier, initialPositionState);
            _hierarchyStates.Add(objSafeRef.Identifier, initialHierarchyState);
        }

        protected override void OnStopRecordingObject(ObjectSafeRef<UnityEngine.Transform> objSafeRef, Record record,
            RecorderContext recorderContext)
        {
            _transformAccessArray.RemoveSwapBack(objSafeRef);
            _gameObjectIdentifiers.Remove(objSafeRef.Identifier);
            _hierarchyStates.Remove(objSafeRef.Identifier);
            _positionStates.Remove(objSafeRef.Identifier);
        }

        private void OnSetParent(UnityEngine.Transform t, UnityEngine.Transform parent, RecorderContext ctx)
        {
            if (!IsRecording)
                return;

            var tSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateTypedObjectSafeRef(t);

            if (!IsRecordingObject(tSafeRef))
                return;

            var parentSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateTypedObjectSafeRef(parent);
            var parentGameObjectSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateTypedObjectSafeRef(parent.gameObject);
            var parentIdentifier =
                new TransformGameObjectIdentifier(parentSafeRef.Identifier, parentGameObjectSafeRef.Identifier);

            // var newState = _positionStates[tSafeRef.Identifier];
            // newState.ParentIdentifier = parentIdentifier;
            // _positionStates[tSafeRef.Identifier] = newState;
        }

        protected override void OnStopRecording(Record record, RecorderContext recorderContext)
        {
            _transformPositionStateUpdater.Complete();
            _transformAccessArray.Clear();
            _gameObjectIdentifiers.Clear();
            _hierarchyStates.Clear();
            _positionStates.Clear();
        }

        protected override void OnEarlyUpdate(long deltaTime, Record record, RecorderContext context)
        {
            _transformPositionStateUpdater.StartPollingPositions();
        }

        protected override TransformFrameData CollectFrameData(FrameInfo frameInfo, Record record,
            RecorderContext context)
        {
            _transformPositionStateUpdater.Complete();
            _transformPositionStateUpdater.MergePolledPositions();

            var updateSamples = new NativeList<TransformUpdate>(RecordedObjects.Count, Allocator.Persistent);
            var createSamples = new NativeList<TransformCreate>(CreatedObjectsIdentifier.Count, Allocator.Persistent);
            var destroySamples =
                new NativeList<TransformDestroy>(DestroyedObjectsIdentifier.Count, Allocator.Persistent);

            var recordedObjectsIdentifier = RecordedObjectsIdentifier.ToNativeArray(Allocator.Persistent);

            new SampleProducerJob
            {
                CreatedInFrameIdentifiers = CreatedObjectsIdentifier,
                DestroyedInFrameIdentifiers = DestroyedObjectsIdentifier,
                TransformIdentifiers = recordedObjectsIdentifier,
                GameObjectsIdentifiers = _gameObjectIdentifiers,
                PositionStates = _positionStates,
                HierarchyStates = _hierarchyStates,
                UpdateSamples = updateSamples.AsParallelWriter(),
                CreateSamples = createSamples.AsParallelWriter(),
                DestroySamples = destroySamples.AsParallelWriter()
            }.RunBatch(RecordedObjects.Count);

            recordedObjectsIdentifier.Dispose();

            return new TransformFrameData(updateSamples, createSamples, destroySamples);
        }
    }
}