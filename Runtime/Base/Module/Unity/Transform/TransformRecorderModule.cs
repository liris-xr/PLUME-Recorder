using PLUME.Base.Hooks;
using PLUME.Base.Module.Unity.Transform.Job;
using PLUME.Base.Module.Unity.Transform.Sample;
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
        private NativeList<TransformState> _alignedStates;

        private TransformPositionStateUpdater _transformPositionStateUpdater;
        
        private int _nCreatedInFrame;
        private int _nDestroyedInFrame;

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
            _alignedStates = new NativeList<TransformState>(1000, Allocator.Persistent);

            GameObjectHooks.OnComponentAdded += (go, comp) => OnComponentAdded(go, comp, ctx);
            TransformHooks.OnParentChanged += (t, parent) => OnParentChanged(t, parent, ctx);
            TransformHooks.OnSiblingIndexChanged += (t, siblingIdx) => OnSiblingIndexChanged(t, siblingIdx, ctx);
        }

        private void OnComponentAdded(UnityEngine.GameObject go, Component component, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            if (component is not UnityEngine.Transform t)
                return;

            var tSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(t);

            if (IsRecordingObject(tSafeRef))
                return;

            StartRecordingObject(tSafeRef, true, ctx);
        }

        protected override void OnDestroy(RecorderContext ctx)
        {
            base.OnDestroy(ctx);

            _transformAccessArray.Dispose();
            _identifierToIndex.Dispose();
            _alignedStates.Dispose();
        }

        protected override void OnObjectMarkedCreated(TransformSafeRef tSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(tSafeRef, ctx);

            var idx = _identifierToIndex[tSafeRef.Identifier];
            
            var state = _alignedStates[idx];
            state.Status = TransformState.LifeStatus.AliveCreatedInFrame;
            _alignedStates[idx] = state;
            
            _nCreatedInFrame++;
        }

        protected override void OnObjectMarkedDestroyed(TransformSafeRef tSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedDestroyed(tSafeRef, ctx);

            var idx = _identifierToIndex[tSafeRef.Identifier];
            
            var state = _alignedStates[idx];
            state.Status = TransformState.LifeStatus.DestroyedInFrame;
            _alignedStates[idx] = state;
            
            _nDestroyedInFrame++;
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

            var initialState = new TransformState
            {
                LocalPosition = localPosition,
                LocalRotation = localRotation,
                LocalScale = localScale,

                ParentTransformId = parentIdentifier,
                SiblingIndex = siblingIndex,

                Status = TransformState.LifeStatus.Alive
            };

            var idx = _transformAccessArray.Length;
            _transformAccessArray.Add(tSafeRef);
            _identifierToIndex.Add(tSafeRef.Identifier, idx);
            _alignedStates.Add(initialState);
        }

        protected override void OnStopRecordingObject(TransformSafeRef tSafeRef, RecorderContext ctx)
        {
            base.OnStopRecordingObject(tSafeRef, ctx);
            var lastIdentifier = _transformAccessArray.GetAlignedIdentifiers()[^1];
            
            var idx = _transformAccessArray.RemoveSwapBack(tSafeRef);
            _alignedStates.RemoveAtSwapBack(idx);
            
            // Update the index of the swapped element
            _identifierToIndex[lastIdentifier] = idx;
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
            var state = _alignedStates[idx];
            state.ParentTransformIdDirty = !parentIdentifier.Equals(state.ParentTransformId);
            state.ParentTransformId = parentIdentifier;
            _alignedStates[idx] = state;
        }

        private void OnSiblingIndexChanged(UnityEngine.Transform t, int siblingIndex, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var tSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(t);

            if (!IsRecordingObject(tSafeRef))
                return;

            var idx = _identifierToIndex[tSafeRef.Identifier];
            var state = _alignedStates[idx];
            state.SiblingIndexDirty = siblingIndex != state.SiblingIndex;
            state.SiblingIndex = siblingIndex;
            _alignedStates[idx] = state;
        }

        protected override void OnStopRecording(RecorderContext ctx)
        {
            base.OnStopRecording(ctx);
            _transformAccessArray.Clear();
            _identifierToIndex.Clear();
            _alignedStates.Clear();
            _nCreatedInFrame = 0;
            _nDestroyedInFrame = 0;
        }

        protected override TransformFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            _transformPositionStateUpdater.UpdatePositionStates(_alignedStates, _transformAccessArray);

            var nRecorded = RecordedComponents.Count;
            var updateSamples = new NativeList<TransformUpdate>(nRecorded, Allocator.Persistent);
            var createSamples = new NativeList<TransformCreate>(_nCreatedInFrame, Allocator.Persistent);
            var destroySamples = new NativeList<TransformDestroy>(_nDestroyedInFrame, Allocator.Persistent);

            new SampleProducerJob
            {
                Identifiers = _transformAccessArray.GetAlignedIdentifiers().AsArray(),
                AlignedStates = _alignedStates.AsArray(),
                UpdateSamples = updateSamples.AsParallelWriter(),
                CreateSamples = createSamples.AsParallelWriter(),
                DestroySamples = destroySamples.AsParallelWriter()
            }.RunBatch(RecordedComponents.Count);

            return new TransformFrameData(updateSamples, createSamples, destroySamples);
        }
        
        protected override void AfterCollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            base.AfterCollectFrameData(frameInfo, ctx);
            _nCreatedInFrame = 0;
            _nDestroyedInFrame = 0;
        }
    }
}