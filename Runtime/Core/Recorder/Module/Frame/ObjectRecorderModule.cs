using System;
using System.Collections.Generic;
using PLUME.Core.Object;
using PLUME.Core.Object.SafeRef;

namespace PLUME.Core.Recorder.Module.Frame
{
    public abstract class ObjectRecorderModule<TObjectIdentifier, TObjectSafeRef, TFrameData> :
        FrameDataRecorderModule<TFrameData>, IObjectRecorderModule
        where TObjectIdentifier : unmanaged, IObjectIdentifier, IEquatable<TObjectIdentifier>
        where TObjectSafeRef : IObjectSafeRef<TObjectIdentifier>
        where TFrameData : IFrameData
    {
        private readonly List<TObjectSafeRef> _recordedObjects = new();

        // Index of each recorded object in _recordedObjects, for constant-time lookup and removal.
        private readonly Dictionary<TObjectIdentifier, int> _recordedObjectIndices = new(1000);

        protected IReadOnlyList<TObjectSafeRef> RecordedObjects => _recordedObjects;

        /// <summary>
        /// List of objects to remove from the recorded objects list after frame data collection.
        /// </summary>
        private readonly List<TObjectSafeRef> _objectsToStopRecording = new();

        private readonly HashSet<TObjectIdentifier> _objectsToStopRecordingIdentifiers = new();

        // Objects destroyed without the hooks seeing it are found by checking a few of them each frame, round-robin,
        // at a constant cost whatever the number of recorded objects.
        private const int LivenessChecksPerFrame = 16;
        private int _nextLivenessCheck;

        protected override void OnStopRecording(RecorderContext ctx)
        {
            base.OnStopRecording(ctx);
            _recordedObjects.Clear();
            _recordedObjectIndices.Clear();
            _objectsToStopRecording.Clear();
            _objectsToStopRecordingIdentifiers.Clear();
        }

        public bool StartRecordingObject(IObjectSafeRef objSafeRef, bool markCreated, RecorderContext ctx)
        {
            CheckIsRecording(ctx);

            if (objSafeRef is not TObjectSafeRef tObjSafeRef)
                return false;

            return StartRecordingObject(tObjSafeRef, markCreated, ctx);
        }

        public bool StopRecordingObject(IObjectSafeRef objSafeRef, bool markDestroyed, RecorderContext ctx)
        {
            CheckIsRecording(ctx);

            if (objSafeRef is not TObjectSafeRef tObjSafeRef)
                return false;

            return StopRecordingObject(tObjSafeRef, markDestroyed, ctx);
        }

        public bool StartRecordingObject(TObjectSafeRef objSafeRef, bool markCreated, RecorderContext ctx)
        {
            CheckIsRecording(ctx);

            // Null safe refs are produced for objects that cannot be identified (e.g. components on
            // hidden HideAndDontSave GameObjects that are not part of a valid scene, such as the HDRP
            // default camera/light singletons). Recording them would dereference a null Object.
            if (objSafeRef.IsNull)
                return false;

            // If we fail to add the object, this means that it is already being recorded.
            if (!_recordedObjectIndices.TryAdd(objSafeRef.Identifier, _recordedObjects.Count))
                return false;

            _recordedObjects.Add(objSafeRef);

            try
            {
                OnStartRecordingObject(objSafeRef, ctx);
            }
            catch (Exception e)
            {
                // Roll back so that the module never iterates over a half-registered object.
                RemoveRecordedObject(objSafeRef.Identifier);
                ModuleFaults.ReportObjectFailure(this, nameof(OnStartRecordingObject), e);
                return false;
            }

            if (markCreated)
            {
                try
                {
                    OnObjectMarkedCreated(objSafeRef, ctx);
                }
                catch (Exception e)
                {
                    // The object stays recorded: only its creation sample is missing.
                    ModuleFaults.ReportObjectFailure(this, nameof(OnObjectMarkedCreated), e);
                }
            }

            return true;
        }

        public bool StopRecordingObject(TObjectSafeRef objSafeRef, bool markDestroyed, RecorderContext ctx)
        {
            CheckIsRecording(ctx);

            // Not recorded, or already stopping in this frame.
            if (!_recordedObjectIndices.ContainsKey(objSafeRef.Identifier) ||
                !_objectsToStopRecordingIdentifiers.Add(objSafeRef.Identifier))
                return false;

            // Deferred removal (after frame data collection)
            _objectsToStopRecording.Add(objSafeRef);

            if (markDestroyed)
            {
                try
                {
                    OnObjectMarkedDestroyed(objSafeRef, ctx);
                }
                catch (Exception e)
                {
                    ModuleFaults.ReportObjectFailure(this, nameof(OnObjectMarkedDestroyed), e);
                }
            }

            return true;
        }

        void IObjectRecorderModule.StopRecordingDestroyedObjects(RecorderContext ctx)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _recordedObjects.Count; i++)
            {
                var objSafeRef = _recordedObjects[i];
                if (IsDestroyed(objSafeRef))
                    StopRecordingObject(objSafeRef, true, ctx);
            }
        }

        protected override void BeforeCollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            base.BeforeCollectFrameData(frameInfo, ctx);

            var checks = Math.Min(LivenessChecksPerFrame, _recordedObjects.Count);
            for (var i = 0; i < checks; i++)
            {
                _nextLivenessCheck = (_nextLivenessCheck + 1) % _recordedObjects.Count;
                var objSafeRef = _recordedObjects[_nextLivenessCheck];

                if (IsDestroyed(objSafeRef) && !IsStoppingRecordingObject(objSafeRef.Identifier))
                {
                    ctx.RequestDestroyedObjectsSweep();
                    return;
                }
            }
        }

        protected override void AfterCollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            base.AfterCollectFrameData(frameInfo, ctx);

            foreach (var toRemove in _objectsToStopRecording)
            {
                RemoveRecordedObject(toRemove.Identifier);

                try
                {
                    OnStopRecordingObject(toRemove, ctx);
                }
                catch (Exception e)
                {
                    ModuleFaults.ReportObjectFailure(this, nameof(OnStopRecordingObject), e);
                }
            }

            _objectsToStopRecording.Clear();
            _objectsToStopRecordingIdentifiers.Clear();
        }

        private void RemoveRecordedObject(TObjectIdentifier identifier)
        {
            if (!_recordedObjectIndices.Remove(identifier, out var index))
                return;

            // Swap back: move the last object into the removed one's place.
            var lastIndex = _recordedObjects.Count - 1;
            if (index != lastIndex)
            {
                var last = _recordedObjects[lastIndex];
                _recordedObjects[index] = last;
                _recordedObjectIndices[last.Identifier] = index;
            }

            _recordedObjects.RemoveAt(lastIndex);
        }

        protected virtual bool IsDestroyed(TObjectSafeRef objSafeRef)
        {
            return objSafeRef.Object == null;
        }

        /// <summary>
        /// Whether the object was asked to stop being recorded during this frame (it is until the frame is collected).
        /// </summary>
        protected bool IsStoppingRecordingObject(TObjectIdentifier identifier)
        {
            return _objectsToStopRecordingIdentifiers.Contains(identifier);
        }

        public bool IsRecordingObject(TObjectSafeRef objSafeRef)
        {
            return _recordedObjectIndices.ContainsKey(objSafeRef.Identifier);
        }

        protected void CheckIsRecording(RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                throw new InvalidOperationException("Recorder module is not recording.");
        }

        protected virtual void OnStartRecordingObject(TObjectSafeRef objSafeRef, RecorderContext ctx)
        {
        }

        protected virtual void OnStopRecordingObject(TObjectSafeRef objSafeRef, RecorderContext ctx)
        {
        }

        protected virtual void OnObjectMarkedCreated(TObjectSafeRef objSafeRef, RecorderContext ctx)
        {
        }

        protected virtual void OnObjectMarkedDestroyed(TObjectSafeRef objSafeRef, RecorderContext ctx)
        {
        }
    }
}