using PLUME.Base.Hooks;
using PLUME.Base.Module.Unity.Transform.Job;
using PLUME.Base.Module.Unity.Transform.Sample;
using PLUME.Base.Module.Unity.Transform.State;
using PLUME.Base.Settings;
using PLUME.Core.Object;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Scripting;

namespace PLUME.Base.Module.Unity.Transform
{
    [Preserve]
    internal class TransformRecorderModule : ComponentRecorderModule<UnityEngine.Transform, TransformFrameData>
    {
        private DynamicTransformAccessArray _transformAccessArray;

        private NativeHashMap<ComponentIdentifier, TransformPositionState> _positionStates;
        private NativeHashMap<ComponentIdentifier, TransformHierarchyState> _hierarchyStates;

        private TransformPositionStateUpdater _transformPositionStateUpdater;

        protected override void OnCreate(RecorderContext ctx)
        {
            var settings = ctx.SettingsProvider.GetOrCreate<TransformRecorderModuleSettings>();
            var angularThreshold = settings.AngularThreshold;
            var positionThresholdSq = settings.PositionThreshold * settings.PositionThreshold;
            var scaleThresholdSq = settings.ScaleThreshold * settings.ScaleThreshold;

            _transformAccessArray = new DynamicTransformAccessArray();

            _positionStates =
                new NativeHashMap<ComponentIdentifier, TransformPositionState>(1000, Allocator.Persistent);
            _hierarchyStates =
                new NativeHashMap<ComponentIdentifier, TransformHierarchyState>(1000, Allocator.Persistent);

            _transformPositionStateUpdater = new TransformPositionStateUpdater(_positionStates, _transformAccessArray,
                angularThreshold, positionThresholdSq, scaleThresholdSq);

            TransformHooks.OnSetParent += (t, parent) => OnSetParent(t, parent, ctx);
            TransformHooks.OnSetSiblingIndex += (t, siblingIdx) => OnSetSiblingIndex(t, siblingIdx, ctx);
        }

        protected override void OnDestroy(RecorderContext ctx)
        {
            _transformPositionStateUpdater.Dispose();
            _transformAccessArray.Dispose();
            _positionStates.Dispose();
            _hierarchyStates.Dispose();
        }

        protected override void OnStartRecordingObject(ComponentSafeRef<UnityEngine.Transform> objSafeRef,
            Record record,
            RecorderContext ctx)
        {
            var t = objSafeRef.Component;

            var siblingIndex = t.GetSiblingIndex();
            var localScale = t.localScale;
            t.GetLocalPositionAndRotation(out var localPosition, out var localRotation);

            var parentSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(t.parent);
            var parentIdentifier = parentSafeRef.ComponentIdentifier;
            
            var initialPositionState = new TransformPositionState
            {
                LocalPosition = localPosition,
                LocalRotation = localRotation,
                LocalScale = localScale
            };

            var initialHierarchyState = new TransformHierarchyState
            {
                ParentIdentifier = parentIdentifier,
                SiblingIndex = siblingIndex
            };

            _transformAccessArray.Add(objSafeRef);
            _positionStates.Add(objSafeRef.ComponentIdentifier, initialPositionState);
            _hierarchyStates.Add(objSafeRef.ComponentIdentifier, initialHierarchyState);
        }

        protected override void OnStopRecordingObject(ComponentSafeRef<UnityEngine.Transform> objSafeRef, Record record,
            RecorderContext recorderContext)
        {
            _transformAccessArray.RemoveSwapBack(objSafeRef);
            _hierarchyStates.Remove(objSafeRef.ComponentIdentifier);
            _positionStates.Remove(objSafeRef.ComponentIdentifier);
        }

        private void OnSetParent(UnityEngine.Transform t, UnityEngine.Transform parent, RecorderContext ctx)
        {
            if (!IsRecording)
                return;

            var tSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(t);

            if (!IsRecordingObject(tSafeRef))
                return;

            var parentSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(parent);
            var parentIdentifier = parentSafeRef.ComponentIdentifier;

            var hierarchyState = _hierarchyStates[tSafeRef.ComponentIdentifier];
            hierarchyState.ParentDirty = !parentIdentifier.Equals(hierarchyState.ParentIdentifier);
            hierarchyState.ParentIdentifier = parentIdentifier;
            _hierarchyStates[tSafeRef.ComponentIdentifier] = hierarchyState;
        }

        private void OnSetSiblingIndex(UnityEngine.Transform t, int siblingIndex, RecorderContext ctx)
        {
            if (!IsRecording)
                return;

            var tSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(t);

            if (!IsRecordingObject(tSafeRef))
                return;

            var hierarchyState = _hierarchyStates[tSafeRef.ComponentIdentifier];
            hierarchyState.SiblingIndexDirty = siblingIndex != hierarchyState.SiblingIndex;
            hierarchyState.SiblingIndex = siblingIndex;
            _hierarchyStates[tSafeRef.ComponentIdentifier] = hierarchyState;
        }

        protected override void OnStopRecording(Record record, RecorderContext recorderContext)
        {
            _transformPositionStateUpdater.Complete();
            _transformAccessArray.Clear();
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

            var updateSamples =
                new NativeList<TransformUpdate>(RecordedComponents.Count, Allocator.Persistent);
            var createSamples =
                new NativeList<TransformCreate>(CreatedComponentsIdentifier.Count, Allocator.Persistent);
            var destroySamples =
                new NativeList<TransformDestroy>(DestroyedComponentsIdentifier.Count, Allocator.Persistent);

            var recordedObjectsIdentifier = RecordedComponentsIdentifier.ToNativeArray(Allocator.Persistent);

            new SampleProducerJob
            {
                CreatedInFrameIdentifiers = CreatedComponentsIdentifier,
                DestroyedInFrameIdentifiers = DestroyedComponentsIdentifier,
                Identifiers = recordedObjectsIdentifier,
                PositionStates = _positionStates,
                HierarchyStates = _hierarchyStates,
                UpdateSamples = updateSamples.AsParallelWriter(),
                CreateSamples = createSamples.AsParallelWriter(),
                DestroySamples = destroySamples.AsParallelWriter()
            }.RunBatch(RecordedComponents.Count);

            recordedObjectsIdentifier.Dispose();

            return new TransformFrameData(updateSamples, createSamples, destroySamples);
        }
    }
}