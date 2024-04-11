using System;
using System.Collections.Generic;
using PLUME.Core.Object;
using PLUME.Core.Object.SafeRef;
using Unity.Collections;

namespace PLUME.Core.Recorder.Module.Frame
{
    public abstract class ObjectRecorderModule<TObjectIdentifier, TObjectSafeRef, TFrameData> :
        FrameDataRecorderModule<TFrameData>, IObjectRecorderModule
        where TObjectIdentifier : unmanaged, IObjectIdentifier, IEquatable<TObjectIdentifier>
        where TObjectSafeRef : IObjectSafeRef<TObjectIdentifier>
        where TFrameData : IFrameData
    {
        private readonly List<TObjectSafeRef> _recordedObjects = new();

        private readonly HashSet<TObjectIdentifier> _recordedObjectsIdentifier = new(1000);

        protected IReadOnlyList<TObjectSafeRef> RecordedObjects => _recordedObjects;

        /// <summary>
        /// List of objects to remove from the recorded objects list after frame data collection.
        /// </summary>
        private readonly List<TObjectSafeRef> _objectsToStopRecording = new();

        protected override void OnStopRecording(RecorderContext ctx)
        {
            base.OnStopRecording(ctx);
            _recordedObjects.Clear();
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

            // If we fail to add the object, this means that it is already being recorded.
            if (!_recordedObjectsIdentifier.Add(objSafeRef.Identifier))
                return false;

            _recordedObjects.Add(objSafeRef);
            OnStartRecordingObject(objSafeRef, ctx);

            if (markCreated)
            {
                OnObjectMarkedCreated(objSafeRef, ctx);
            }

            return true;
        }

        public bool StopRecordingObject(TObjectSafeRef objSafeRef, bool markDestroyed, RecorderContext ctx)
        {
            CheckIsRecording(ctx);

            // If we fail to remove the object, this means that it was not being recorded.
            if (!_recordedObjectsIdentifier.Contains(objSafeRef.Identifier))
                return false;

            // Deferred removal (after frame data collection)
            _objectsToStopRecording.Add(objSafeRef);

            if (markDestroyed)
            {
                OnObjectMarkedDestroyed(objSafeRef, ctx);
            }

            return true;
        }

        protected override void AfterCollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            base.AfterCollectFrameData(frameInfo, ctx);

            foreach (var toRemove in _objectsToStopRecording)
            {
                _recordedObjectsIdentifier.Remove(toRemove.Identifier);
                _recordedObjects.RemoveSwapBack(toRemove);
                OnStopRecordingObject(toRemove, ctx);
            }

            _objectsToStopRecording.Clear();
        }

        public bool IsRecordingObject(TObjectSafeRef objSafeRef)
        {
            return _recordedObjectsIdentifier.Contains(objSafeRef.Identifier);
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