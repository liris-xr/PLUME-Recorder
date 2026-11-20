using System.Collections.Generic;
using PLUME.Base.Hooks;
using PLUME.Base.Module.Unity.Transform.Job;
using PLUME.Base.Settings;
using PLUME.Core.Object;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.ProtoBurst.Unity;
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
        
        // The recorded transform of each GameObject, by GameObject runtime id.
        private readonly Dictionary<int, TransformSafeRef> _transformByGameObject = new();

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

            TransformHooks.OnParentChanged += (t, parent) => OnParentChanged(t, parent, ctx);
            TransformHooks.OnSiblingIndexChanged += (t, siblingIdx) => OnSiblingIndexChanged(t, siblingIdx, ctx);
            GameObjectHooks.OnBeforeTransformReplaced += go => OnBeforeTransformReplaced(go, ctx);
            GameObjectHooks.OnComponentAdded += (go, _) => OnComponentAdded(go, ctx);
        }

        private void OnBeforeTransformReplaced(UnityEngine.GameObject go, RecorderContext ctx)
        {
            if (!ctx.IsRecordingWith(this) || !_transformByGameObject.TryGetValue(go.GetInstanceID(), out var tSafeRef))
                return;

            // Must leave the TransformAccessArray now, not at the end of the frame: see OnBeforeTransformReplaced.
            // shortcut: if AddComponent then fails without replacing it, the transform stops being recorded. Unity only
            // refuses conflicting components, which a GameObject without RectTransform hardly has; restart it if seen.
            RemoveFromArrays(tSafeRef);
            StopRecordingObject(tSafeRef, false, ctx);
        }

        /// <summary>
        /// Adding a UI component (e.g. an Image) to a GameObject makes Unity replace its Transform by a RectTransform,
        /// which is not notified when only required indirectly: the new transform is recorded by all the modules, as
        /// the old one's successor.
        /// </summary>
        private void OnComponentAdded(UnityEngine.GameObject go, RecorderContext ctx)
        {
            if (!ctx.IsRecordingWith(this) || !_transformByGameObject.ContainsKey(go.GetInstanceID()))
                return;

            var tSafeRef = ctx.SafeRefProvider.GetOrCreateComponentSafeRef(go.transform);

            if (!IsRecordingObject(tSafeRef))
                ctx.StartRecordingObjectInternal(tSafeRef, true);
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

            var parentSafeRef = ctx.SafeRefProvider.GetOrCreateComponentSafeRef(t.parent);
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

            // A GameObject has a single transform: a different one recorded for it was replaced, and is gone. It
            // shares the new one's identity, so it stops being recorded without being marked destroyed.
            var gameObjectId = tSafeRef.Identifier.GameObject.RuntimeId;
            if (_transformByGameObject.TryGetValue(gameObjectId, out var replaced))
                StopRecordingObject(replaced, false, ctx);
            _transformByGameObject[gameObjectId] = tSafeRef;
        }

        protected override void OnStopRecordingObject(TransformSafeRef tSafeRef, RecorderContext ctx)
        {
            base.OnStopRecordingObject(tSafeRef, ctx);

            var gameObjectId = tSafeRef.Identifier.GameObject.RuntimeId;
            if (_transformByGameObject.TryGetValue(gameObjectId, out var current) && current == tSafeRef)
                _transformByGameObject.Remove(gameObjectId);

            RemoveFromArrays(tSafeRef);
        }

        private void RemoveFromArrays(TransformSafeRef tSafeRef)
        {
            // Already removed, e.g. before its replacement by a RectTransform
            if (!_transformAccessArray.TryRemoveSwapBack(tSafeRef, out var idx))
                return;

            _alignedStates.RemoveAtSwapBack(idx);
            _identifierToIndex.Remove(tSafeRef.Identifier);

            // The last element was swapped into the removed one's place, unless the removed one was the last
            if (idx < _transformAccessArray.Length)
                _identifierToIndex[_transformAccessArray.GetAlignedIdentifiers()[idx]] = idx;
        }

        private void OnParentChanged(UnityEngine.Transform t, UnityEngine.Transform parent, RecorderContext ctx)
        {
            if (!ctx.IsRecordingWith(this))
                return;

            var tSafeRef = ctx.SafeRefProvider.GetOrCreateComponentSafeRef(t);

            if (!IsRecordingObject(tSafeRef))
                return;

            var parentSafeRef = ctx.SafeRefProvider.GetOrCreateComponentSafeRef(parent);
            var parentIdentifier = parentSafeRef.Identifier;

            var idx = _identifierToIndex[tSafeRef.Identifier];
            var state = _alignedStates[idx];
            state.ParentTransformIdDirty = !parentIdentifier.Equals(state.ParentTransformId);
            state.ParentTransformId = parentIdentifier;
            _alignedStates[idx] = state;
        }

        private void OnSiblingIndexChanged(UnityEngine.Transform t, int siblingIndex, RecorderContext ctx)
        {
            if (!ctx.IsRecordingWith(this))
                return;

            var tSafeRef = ctx.SafeRefProvider.GetOrCreateComponentSafeRef(t);

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
            _transformByGameObject.Clear();
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
            }.RunBatch(_alignedStates.Length);

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