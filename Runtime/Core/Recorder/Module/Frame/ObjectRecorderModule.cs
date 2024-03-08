using System;
using System.Collections.Generic;
using PLUME.Core.Object;
using PLUME.Core.Object.SafeRef;
using Unity.Collections;

namespace PLUME.Core.Recorder.Module.Frame
{
    public abstract class ObjectRecorderModule<TObject, TObjectIdentifier, TObjectSafeRef, TFrameData> :
        FrameDataRecorderModule<TFrameData>, IObjectRecorderModule
        where TObject : UnityEngine.Object
        where TObjectIdentifier : unmanaged, IObjectIdentifier, IEquatable<TObjectIdentifier>
        where TObjectSafeRef : IObjectSafeRef<TObjectIdentifier>
        where TFrameData : IFrameData
    {
        private readonly List<TObjectSafeRef> _recordedObjects = new();
        private NativeHashSet<TObjectIdentifier> _recordedObjectsIdentifier;
        private NativeHashSet<TObjectIdentifier> _createdObjectsIdentifier;
        private NativeHashSet<TObjectIdentifier> _destroyedObjectsIdentifier;

        protected IReadOnlyList<TObjectSafeRef> RecordedObjects => _recordedObjects;

        protected NativeHashSet<TObjectIdentifier>.ReadOnly RecordedObjectsIdentifier =>
            _recordedObjectsIdentifier.AsReadOnly();

        protected NativeHashSet<TObjectIdentifier>.ReadOnly CreatedObjectsIdentifier =>
            _createdObjectsIdentifier.AsReadOnly();

        protected NativeHashSet<TObjectIdentifier>.ReadOnly DestroyedObjectsIdentifier =>
            _destroyedObjectsIdentifier.AsReadOnly();

        /// <summary>
        /// List of objects to remove from the recorded objects list after frame data collection.
        /// </summary>
        private readonly List<TObjectSafeRef> _objectsToRemove = new();

        protected override void OnCreate(RecorderContext ctx)
        {
            base.OnCreate(ctx);
            _recordedObjectsIdentifier = new NativeHashSet<TObjectIdentifier>(1000, Allocator.Persistent);
            _createdObjectsIdentifier = new NativeHashSet<TObjectIdentifier>(1000, Allocator.Persistent);
            _destroyedObjectsIdentifier = new NativeHashSet<TObjectIdentifier>(1000, Allocator.Persistent);
        }
        
        protected override void OnDestroy(RecorderContext ctx)
        {
            base.OnDestroy(ctx);
            _recordedObjectsIdentifier.Dispose();
            _createdObjectsIdentifier.Dispose();
            _destroyedObjectsIdentifier.Dispose();
        }
        
        protected override void OnStopRecording(RecorderContext ctx)
        {
            base.OnStopRecording(ctx);
            _recordedObjects.Clear();
            _createdObjectsIdentifier.Clear();
            _destroyedObjectsIdentifier.Clear();
        }
        
        public void StartRecordingObject(IObjectSafeRef objSafeRef, bool markCreated, RecorderContext ctx)
        {
            CheckIsRecording(ctx);
            
            if (objSafeRef is not TObjectSafeRef tObjSafeRef)
            {
                throw new InvalidOperationException($"Object is not of type {typeof(TObject).Name}");
            }

            StartRecordingObject(tObjSafeRef, markCreated, ctx);
        }

        public void StopRecordingObject(IObjectSafeRef objSafeRef, bool markDestroyed, RecorderContext ctx)
        {
            CheckIsRecording(ctx);
            
            if (objSafeRef is not TObjectSafeRef tObjSafeRef)
            {
                throw new InvalidOperationException($"Object is not of type {typeof(TObject).Name}");
            }

            StopRecordingObject(tObjSafeRef, markDestroyed, ctx);
        }

        public void StartRecordingObject(TObjectSafeRef objSafeRef, bool markCreated, RecorderContext ctx)
        {
            CheckIsRecording(ctx);

            if (!_recordedObjectsIdentifier.Add(objSafeRef.Identifier))
                throw new InvalidOperationException("Object is already being recorded.");

            _recordedObjects.Add(objSafeRef);
            OnStartRecordingObject(objSafeRef, ctx);

            if (!markCreated)
                return;

            var markedCreated = _createdObjectsIdentifier.Add(objSafeRef.Identifier);
            _destroyedObjectsIdentifier.Remove(objSafeRef.Identifier);

            if (markedCreated)
                OnObjectMarkedCreated(objSafeRef, ctx);
        }

        public void StopRecordingObject(TObjectSafeRef objSafeRef, bool markDestroyed, RecorderContext ctx)
        {
            CheckIsRecording(ctx);

            if (!_recordedObjectsIdentifier.Contains(objSafeRef.Identifier))
                throw new InvalidOperationException("Object is not being recorded.");

            // Deferred removal (after frame data collection)
            _objectsToRemove.Add(objSafeRef);

            if (markDestroyed)
            {
                var markedDestroyed = _destroyedObjectsIdentifier.Add(objSafeRef.Identifier);
                _createdObjectsIdentifier.Remove(objSafeRef.Identifier);

                if (markedDestroyed)
                    OnObjectMarkedDestroyed(objSafeRef, ctx);
            }
        }

        protected override void AfterCollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            base.AfterCollectFrameData(frameInfo, ctx);
            
            _createdObjectsIdentifier.Clear();
            _destroyedObjectsIdentifier.Clear();

            foreach (var toRemove in _objectsToRemove)
            {
                _recordedObjectsIdentifier.Remove(toRemove.Identifier);
                _recordedObjects.Remove(toRemove);
                OnStopRecordingObject(toRemove, ctx);
            }

            _objectsToRemove.Clear();
        }

        public bool IsObjectSupported(IObjectSafeRef objSafeRef)
        {
            return objSafeRef is TObjectSafeRef;
        }

        public bool IsRecordingObject(IObjectSafeRef objSafeRef)
        {
            return objSafeRef is TObjectSafeRef tObjSafeRef && _recordedObjects.Contains(tObjSafeRef);
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