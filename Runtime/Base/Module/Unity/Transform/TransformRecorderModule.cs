using PLUME.Base.Events;
using PLUME.Base.Module.Unity.Transform.Job;
using PLUME.Base.Module.Unity.Transform.Sample;
using PLUME.Base.Module.Unity.Transform.State;
using PLUME.Base.Settings;
using PLUME.Core.Object;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Scripting;
using TransformSafeRef = PLUME.Core.Object.SafeRef.IComponentSafeRef<UnityEngine.Transform>;

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
            base.OnCreate(ctx);
            
            var settings = ctx.SettingsProvider.GetOrCreate<TransformRecorderModuleSettings>();
            var angularThreshold = settings.AngularThreshold;
            var positionThresholdSq = settings.PositionThreshold * settings.PositionThreshold;
            var scaleThresholdSq = settings.ScaleThreshold * settings.ScaleThreshold;
            _transformPositionStateUpdater =
                new TransformPositionStateUpdater(angularThreshold, positionThresholdSq, scaleThresholdSq);

            _transformAccessArray = new DynamicTransformAccessArray();

            _identifierToIndex = new NativeHashMap<ComponentIdentifier, int>(1000, Allocator.Persistent);
            _alignedPositionStates = new NativeList<TransformPositionState>(1000, Allocator.Persistent);
            _alignedHierarchyStates = new NativeList<TransformHierarchyState>(1000, Allocator.Persistent);
            _alignedFlagsStates = new NativeList<TransformFlagsState>(1000, Allocator.Persistent);
            
            GameObjectEvents.OnComponentAdded += (go, comp) => OnComponentAdded(go, comp, ctx);
            TransformEvents.OnParentChanged += (t, parent) => OnParentChanged(t, parent, ctx);
            TransformEvents.OnSiblingIndexChanged += (t, siblingIdx) => OnSiblingIndexChanged(t, siblingIdx, ctx);
        }

        private void OnComponentAdded(UnityEngine.GameObject go, Component component, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;
            
            if(component is not UnityEngine.Transform t)
                return;

            var tSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(t);

            if (!IsRecordingObject(tSafeRef))
                return;

            StartRecordingObject(tSafeRef, true, ctx);
        }

        protected override void OnDestroy(RecorderContext ctx)
        {
            base.OnDestroy(ctx);
            
            _transformAccessArray.Dispose();
            _identifierToIndex.Dispose();
            _alignedPositionStates.Dispose();
            _alignedHierarchyStates.Dispose();
            _alignedFlagsStates.Dispose();
        }

        protected override void OnObjectMarkedCreated(TransformSafeRef tSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(tSafeRef, ctx);
            
            var idx = _identifierToIndex[tSafeRef.Identifier];
            var flagsState = _alignedFlagsStates[idx];
            flagsState.MarkCreatedInFrame();
            _alignedFlagsStates[idx] = flagsState;
        }

        protected override void OnObjectMarkedDestroyed(TransformSafeRef tSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedDestroyed(tSafeRef, ctx);
            
            var idx = _identifierToIndex[tSafeRef.Identifier];
            var flagsState = _alignedFlagsStates[idx];
            flagsState.MarkDestroyedInFrame();
            _alignedFlagsStates[idx] = flagsState;
        }

        protected override void OnStartRecordingObject(TransformSafeRef tSafeRef, RecorderContext ctx)
        {
            base.OnStartRecordingObject(tSafeRef, ctx);
            
            var t = tSafeRef.Component;

            var siblingIndex = t.GetSiblingIndex();
            var localScale = t.localScale;
            t.GetLocalPositionAndRotation(out var localPosition, out var localRotation);

            var parentSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(t.parent);
            var parentIdentifier = parentSafeRef.Identifier;
            
            var initialPositionState = new TransformPositionState
            {
                LocalPosition = localPosition,
                LocalRotation = localRotation,
                LocalScale = localScale
            };

            var initialHierarchyState = new TransformHierarchyState
            {
                ParentTransformId = parentIdentifier,
                SiblingIndex = siblingIndex
            };

            var initialFlagsState = new TransformFlagsState();

            var idx = _transformAccessArray.Length;
            _transformAccessArray.Add(tSafeRef);
            _identifierToIndex.Add(tSafeRef.Identifier, idx);
            _alignedPositionStates.Add(initialPositionState);
            _alignedHierarchyStates.Add(initialHierarchyState);
            _alignedFlagsStates.Add(initialFlagsState);
        }

        protected override void OnStopRecordingObject(TransformSafeRef tSafeRef, RecorderContext ctx)
        {
            base.OnStopRecordingObject(tSafeRef, ctx);
            var idx = _transformAccessArray.RemoveSwapBack(tSafeRef);
            _identifierToIndex.Remove(tSafeRef.Identifier);
            _alignedHierarchyStates.RemoveAtSwapBack(idx);
            _alignedPositionStates.RemoveAtSwapBack(idx);
            _alignedFlagsStates.RemoveAtSwapBack(idx);
        }

        private void OnParentChanged(UnityEngine.Transform t, UnityEngine.Transform parent, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var tSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(t);

            if (!IsRecordingObject(tSafeRef))
                return;

            var parentSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(parent);
            var parentIdentifier = parentSafeRef.Identifier;
            
            var idx = _identifierToIndex[tSafeRef.Identifier];
            var hierarchyState = _alignedHierarchyStates[idx];
            hierarchyState.ParentTransformIdDirty = !parentIdentifier.Equals(hierarchyState.ParentTransformId);
            hierarchyState.ParentTransformId = parentIdentifier;
            _alignedHierarchyStates[idx] = hierarchyState;
        }

        private void OnSiblingIndexChanged(UnityEngine.Transform t, int siblingIndex, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var tSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(t);

            if (!IsRecordingObject(tSafeRef))
                return;

            var idx = _identifierToIndex[tSafeRef.Identifier];
            var hierarchyState = _alignedHierarchyStates[idx];
            hierarchyState.SiblingIndexDirty = siblingIndex != hierarchyState.SiblingIndex;
            hierarchyState.SiblingIndex = siblingIndex;
            _alignedHierarchyStates[idx] = hierarchyState;
        }

        protected override void OnStopRecording(RecorderContext ctx)
        {
            base.OnStopRecording(ctx);
            _transformAccessArray.Clear();
            _identifierToIndex.Clear();
            _alignedHierarchyStates.Clear();
            _alignedPositionStates.Clear();
            _alignedFlagsStates.Clear();
        }

        protected override TransformFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            _transformPositionStateUpdater.UpdatePositionStates(_alignedPositionStates, _transformAccessArray);

            var nRecorded = RecordedComponents.Count;
            var nCreated = CreatedComponentsIdentifier.Count;
            var nDestroyed = DestroyedComponentsIdentifier.Count;
            var updateSamples = new NativeList<TransformUpdate>(nRecorded, Allocator.Persistent);
            var createSamples = new NativeList<TransformCreate>(nCreated, Allocator.Persistent);
            var destroySamples = new NativeList<TransformDestroy>(nDestroyed, Allocator.Persistent);

            new SampleProducerJob
            {
                Identifiers = _transformAccessArray.GetAlignedIdentifiers().AsArray(),
                AlignedPositionStates = _alignedPositionStates.AsArray(),
                AlignedHierarchyStates = _alignedHierarchyStates.AsArray(),
                AlignedFlagsStates = _alignedFlagsStates.AsArray(),
                UpdateSamples = updateSamples.AsParallelWriter(),
                CreateSamples = createSamples.AsParallelWriter(),
                DestroySamples = destroySamples.AsParallelWriter()
            }.RunBatch(RecordedComponents.Count);

            return new TransformFrameData(updateSamples, createSamples, destroySamples);
        }
    }
}