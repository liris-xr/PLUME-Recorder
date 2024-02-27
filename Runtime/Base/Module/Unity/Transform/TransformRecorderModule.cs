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

        private NativeHashMap<ComponentIdentifier, int> _identifierToIndex;
        private NativeList<TransformPositionState> _alignedPositionStates;
        private NativeList<TransformHierarchyState> _alignedHierarchyStates;
        private NativeList<TransformFlagsState> _alignedFlagsStates;

        private TransformPositionStateUpdater _transformPositionStateUpdater;

        protected override void OnCreate(RecorderContext ctx)
        {
            var settings = ctx.SettingsProvider.GetOrCreate<TransformRecorderModuleSettings>();
            var angularThreshold = settings.AngularThreshold;
            var positionThresholdSq = settings.PositionThreshold * settings.PositionThreshold;
            var scaleThresholdSq = settings.ScaleThreshold * settings.ScaleThreshold;

            _transformAccessArray = new DynamicTransformAccessArray();

            _identifierToIndex = new NativeHashMap<ComponentIdentifier, int>(1000, Allocator.Persistent);
            _alignedPositionStates = new NativeList<TransformPositionState>(1000, Allocator.Persistent);
            _alignedHierarchyStates = new NativeList<TransformHierarchyState>(1000, Allocator.Persistent);
            _alignedFlagsStates = new NativeList<TransformFlagsState>(1000, Allocator.Persistent);

            _transformPositionStateUpdater = new TransformPositionStateUpdater(angularThreshold, positionThresholdSq, scaleThresholdSq);

            TransformHooks.OnSetParent += (t, parent) => OnSetParent(t, parent, ctx);
            TransformHooks.OnSetSiblingIndex += (t, siblingIdx) => OnSetSiblingIndex(t, siblingIdx, ctx);
        }

        protected override void OnDestroy(RecorderContext ctx)
        {
            _transformAccessArray.Dispose();
            _identifierToIndex.Dispose();
            _alignedPositionStates.Dispose();
            _alignedHierarchyStates.Dispose();
            _alignedFlagsStates.Dispose();
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

            var initialFlagsState = new TransformFlagsState();

            var idx = _transformAccessArray.Length;
            _transformAccessArray.Add(objSafeRef);
            _identifierToIndex.Add(objSafeRef.ComponentIdentifier, idx);
            _alignedPositionStates.Add(initialPositionState);
            _alignedHierarchyStates.Add(initialHierarchyState);
            _alignedFlagsStates.Add(initialFlagsState);
        }

        protected override void OnStopRecordingObject(ComponentSafeRef<UnityEngine.Transform> objSafeRef, Record record,
            RecorderContext recorderContext)
        {
            var idx = _transformAccessArray.RemoveSwapBack(objSafeRef);
            _identifierToIndex.Remove(objSafeRef.ComponentIdentifier);
            _alignedHierarchyStates.RemoveAtSwapBack(idx);
            _alignedPositionStates.RemoveAtSwapBack(idx);
            _alignedFlagsStates.RemoveAtSwapBack(idx);
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

            var idx = _identifierToIndex[tSafeRef.ComponentIdentifier];
            var hierarchyState = _alignedHierarchyStates[idx];
            hierarchyState.ParentDirty = !parentIdentifier.Equals(hierarchyState.ParentIdentifier);
            hierarchyState.ParentIdentifier = parentIdentifier;
            _alignedHierarchyStates[idx] = hierarchyState;
        }

        private void OnSetSiblingIndex(UnityEngine.Transform t, int siblingIndex, RecorderContext ctx)
        {
            if (!IsRecording)
                return;

            var tSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(t);

            if (!IsRecordingObject(tSafeRef))
                return;

            var idx = _identifierToIndex[tSafeRef.ComponentIdentifier];
            var hierarchyState = _alignedHierarchyStates[idx];
            hierarchyState.SiblingIndexDirty = siblingIndex != hierarchyState.SiblingIndex;
            hierarchyState.SiblingIndex = siblingIndex;
            _alignedHierarchyStates[idx] = hierarchyState;
        }

        protected override void OnStopRecording(Record record, RecorderContext recorderContext)
        {
            _transformAccessArray.Clear();
            _identifierToIndex.Clear();
            _alignedHierarchyStates.Clear();
            _alignedPositionStates.Clear();
            _alignedFlagsStates.Clear();
        }

        protected override TransformFrameData CollectFrameData(FrameInfo frameInfo, Record record, RecorderContext ctx)
        {
            _transformPositionStateUpdater.UpdatePositionStates(_alignedPositionStates, _transformAccessArray);

            var nRecorded = RecordedComponents.Count;
            var nCreated = CreatedComponentsIdentifier.Count;
            var nDestroyed = DestroyedComponentsIdentifier.Count;
            var updateSamples = new NativeList<TransformUpdate>(nRecorded, Allocator.Persistent);
            var createSamples = new NativeList<TransformCreate>(nCreated, Allocator.Persistent);
            var destroySamples = new NativeList<TransformDestroy>(nDestroyed, Allocator.Persistent);

            var identifiers = RecordedComponentsIdentifier.ToNativeArray(Allocator.Persistent);

            new SampleProducerJob
            {
                Identifiers = identifiers,
                AlignedPositionStates = _alignedPositionStates.AsArray(),
                AlignedHierarchyStates = _alignedHierarchyStates.AsArray(),
                AlignedFlagsStates = _alignedFlagsStates.AsArray(),
                UpdateSamples = updateSamples.AsParallelWriter(),
                CreateSamples = createSamples.AsParallelWriter(),
                DestroySamples = destroySamples.AsParallelWriter()
            }.RunBatch(RecordedComponents.Count);

            identifiers.Dispose();

            return new TransformFrameData(updateSamples, createSamples, destroySamples);
        }
    }
}