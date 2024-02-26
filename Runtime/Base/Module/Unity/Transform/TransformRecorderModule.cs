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
using UnityEngine.Jobs;
using UnityEngine.Scripting;

namespace PLUME.Base.Module.Unity.Transform
{
    [Preserve]
    internal class
        TransformRecorderModule : ObjectFrameDataRecorderModuleBase<UnityEngine.Transform, TransformFrameData>
    {
        private float _angularThreshold;
        private float _positionThresholdSq;
        private float _scaleThresholdSq;

        private DynamicTransformAccessArray _transformAccessArray;

        private NativeHashMap<ObjectIdentifier, ObjectIdentifier> _gameObjectIdentifiers;
        private NativeHashMap<ObjectIdentifier, PositionState> _positionStates;
        private NativeHashMap<ObjectIdentifier, HierarchyState> _hierarchyStates;

        private NativeArray<ObjectIdentifier> _identifiersWorkCopy;
        private NativeArray<PositionState> _positionStatesWorkCopy;
        private JobHandle _pollNewPositionStatesJobHandle;

        protected override void OnCreate(RecorderContext ctx)
        {
            var positionThreshold = TransformRecorderModuleSettings.GetOrCreate().PositionThreshold;
            var scaleThreshold = TransformRecorderModuleSettings.GetOrCreate().ScaleThreshold;
            _angularThreshold = TransformRecorderModuleSettings.GetOrCreate().AngularThreshold;
            _positionThresholdSq = positionThreshold * positionThreshold;
            _scaleThresholdSq = scaleThreshold * scaleThreshold;

            _transformAccessArray = new DynamicTransformAccessArray();

            _positionStates = new NativeHashMap<ObjectIdentifier, PositionState>(1000, Allocator.Persistent);
            _gameObjectIdentifiers = new NativeHashMap<ObjectIdentifier, ObjectIdentifier>(1000, Allocator.Persistent);
            _hierarchyStates = new NativeHashMap<ObjectIdentifier, HierarchyState>(1000, Allocator.Persistent);

            TransformHooks.OnSetParent += (t, parent) => OnSetParent(t, parent, ctx);
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

        protected override void OnDestroy(RecorderContext ctx)
        {
            _pollNewPositionStatesJobHandle.Complete();

            if (_transformAccessArray.IsCreated)
                _transformAccessArray.Dispose();

            // if (_positionStates.IsCreated)
            //     _positionStates.Dispose();
            //
            // if (_currentFrameData.IsCreated)
            //     _currentFrameData.Dispose();
        }

        protected override void OnStartRecordingObject(ObjectSafeRef<UnityEngine.Transform> objSafeRef, Record record,
            RecorderContext recorderContext)
        {
            if (!_transformAccessArray.TryAdd(objSafeRef)) return;

            var t = objSafeRef.TypedObject;
            var goSafeRef = recorderContext.ObjectSafeRefProvider.GetOrCreateTypedObjectSafeRef(t.gameObject);

            var siblingIndex = t.GetSiblingIndex();
            var localScale = t.localScale;
            t.GetLocalPositionAndRotation(out var localPosition, out var localRotation);

            var parentIdentifier = TransformGameObjectIdentifier.Null;

            if (t.parent != null)
            {
                var parentTransformSafeRef =
                    recorderContext.ObjectSafeRefProvider.GetOrCreateTypedObjectSafeRef(t.parent);
                var parentGoSafeRef = recorderContext.ObjectSafeRefProvider.GetOrCreateTypedObjectSafeRef(t.gameObject);
                parentIdentifier =
                    new TransformGameObjectIdentifier(parentTransformSafeRef.Identifier, parentGoSafeRef.Identifier);
            }

            _gameObjectIdentifiers.Add(objSafeRef.Identifier, goSafeRef.Identifier);

            _positionStates.Add(objSafeRef.Identifier, new PositionState
            {
                LocalPosition = localPosition,
                LocalRotation = localRotation,
                LocalScale = localScale
            });

            _hierarchyStates.Add(objSafeRef.Identifier, new HierarchyState
            {
                ParentIdentifier = parentIdentifier,
                SiblingIndex = siblingIndex
            });
        }

        protected override void OnStopRecordingObject(ObjectSafeRef<UnityEngine.Transform> objSafeRef, Record record, RecorderContext recorderContext)
        {
            _transformAccessArray.RemoveSwapBack(objSafeRef);
            _gameObjectIdentifiers.Remove(objSafeRef.Identifier);
            _hierarchyStates.Remove(objSafeRef.Identifier);
            _positionStates.Remove(objSafeRef.Identifier);
        }

        protected override void OnStopRecording(Record record, RecorderContext recorderContext)
        {
            _pollNewPositionStatesJobHandle.Complete();
            _transformAccessArray.Clear();

            _gameObjectIdentifiers.Clear();
            _hierarchyStates.Clear();
            _positionStates.Clear();
        }

        protected override void OnEarlyUpdate(long deltaTime, Record record, RecorderContext context)
        {
            _identifiersWorkCopy = _positionStates.GetKeyArray(Allocator.Persistent);
            _positionStatesWorkCopy = _positionStates.GetValueArray(Allocator.Persistent);

            var pollTransformStatesJob = new PollPositionStatesJob
            {
                PositionStates = _positionStatesWorkCopy,
                AngularThreshold = _angularThreshold,
                PositionThresholdSq = _positionThresholdSq,
                ScaleThresholdSq = _scaleThresholdSq,
            };

            _pollNewPositionStatesJobHandle = pollTransformStatesJob.ScheduleReadOnly(_transformAccessArray, 128);
        }

        protected override TransformFrameData CollectFrameData(FrameInfo frameInfo, Record record, RecorderContext context)
        {
            _pollNewPositionStatesJobHandle.Complete();

            new MergePositionStatesJob
            {
                PositionStates = _positionStates,
                PositionStatesWorkCopy = _positionStatesWorkCopy,
                IdentifiersWorkCopy = _identifiersWorkCopy
            }.RunBatch(_positionStatesWorkCopy.Length);

            var updateSamples = new NativeList<TransformUpdate>(RecordedObjects.Count, Allocator.Persistent);
            var createSamples = new NativeList<TransformCreate>(CreatedInFrame.Count, Allocator.Persistent);
            var destroySamples = new NativeList<TransformDestroy>(DestroyedInFrame.Count, Allocator.Persistent);

            new SampleProducerJob
            {
                CreatedInFrame = CreatedInFrame,
                DestroyedInFrame = DestroyedInFrame,
                TransformIdentifiers = _transformAccessArray.GetAlignedIdentifiers(),
                GameObjectsIdentifiers = _gameObjectIdentifiers,
                PositionStates = _positionStates,
                HierarchyStates = _hierarchyStates,
                UpdateSamples = updateSamples.AsParallelWriter(),
                CreateSamples = createSamples.AsParallelWriter(),
                DestroySamples = destroySamples.AsParallelWriter()
            }.RunBatch(RecordedObjects.Count);

            _positionStatesWorkCopy.Dispose();
            _identifiersWorkCopy.Dispose();

            return new TransformFrameData(updateSamples, createSamples, destroySamples);
        }
    }
}