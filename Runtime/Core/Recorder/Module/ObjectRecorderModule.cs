using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using PLUME.Core.Object;
using PLUME.Core.Object.SafeRef;

namespace PLUME.Core.Recorder.Module
{
    public abstract class ObjectRecorderModuleBase<TObject> : IObjectRecorderModule where TObject : UnityEngine.Object
    {
        public Type SupportedObjectType => typeof(TObject);
        
        public bool IsRecording { get; private set; }

        private readonly ObjectCollection<TObject> _recordedObjects = new();
        private readonly ObjectCollection<TObject> _createdObjects = new();
        private readonly ObjectCollection<TObject> _destroyedObjects = new();

        protected IReadOnlyList<ObjectSafeRef<TObject>> RecordedObjects => _recordedObjects.AsReadOnly();
        protected IReadOnlyList<ObjectSafeRef<TObject>> CreatedObjects => _createdObjects.AsReadOnly();
        protected IReadOnlyList<ObjectSafeRef<TObject>> DestroyedObjects => _destroyedObjects.AsReadOnly();

        public void StartRecordingObject(ObjectSafeRef<TObject> objectSafeRef, bool markCreated)
        {
            EnsureIsRecording();

            if (!_recordedObjects.TryAdd(objectSafeRef))
                throw new InvalidOperationException("Object is already being recorded.");

            OnStartRecordingObject(objectSafeRef, markCreated);

            if (!markCreated)
                return;

            var successfullyMarkedCreated = _createdObjects.TryAdd(objectSafeRef);
            _destroyedObjects.TryRemove(objectSafeRef);

            if (successfullyMarkedCreated)
                OnMarkedCreated(objectSafeRef);
        }

        public void StopRecordingObject(ObjectSafeRef<TObject> objSafeRef, bool markDestroyed)
        {
            EnsureIsRecording();

            if (!_recordedObjects.TryRemove(objSafeRef))
                throw new InvalidOperationException("Object is not being recorded.");

            OnStopRecordingObject(objSafeRef, markDestroyed);

            if (!markDestroyed)
                return;

            var successfullyMarkedDestroyed = _destroyedObjects.TryRemove(objSafeRef);
            _createdObjects.TryRemove(objSafeRef);

            if (successfullyMarkedDestroyed)
                OnMarkedDestroyed(objSafeRef);
        }

        protected void ClearCreatedObjects()
        {
            _createdObjects.Clear();
        }

        protected void ClearDestroyedObjects()
        {
            _destroyedObjects.Clear();
        }

        public void StartRecordingObject(IObjectSafeRef objSafeRef, bool markCreated)
        {
            if (objSafeRef is not ObjectSafeRef<TObject> tObjSafeRef)
            {
                throw new InvalidOperationException($"Object is not of type {typeof(TObject).Name}");
            }

            StartRecordingObject(tObjSafeRef, markCreated);
        }

        public void StopRecordingObject(IObjectSafeRef objSafeRef, bool markCreated)
        {
            if (objSafeRef is not ObjectSafeRef<TObject> tObjSafeRef)
            {
                throw new InvalidOperationException($"Object is not of type {typeof(TObject).Name}");
            }

            StopRecordingObject(tObjSafeRef, markCreated);
        }

        public bool IsRecordingObject(IObjectSafeRef objSafeRef)
        {
            return objSafeRef is ObjectSafeRef<TObject> tObjSafeRef && _recordedObjects.Contains(tObjSafeRef);
        }

        void IRecorderModule.Create(RecorderContext recorderContext)
        {
            OnCreate(recorderContext);
        }

        void IRecorderModule.Destroy(RecorderContext recorderContext)
        {
            OnDestroy(recorderContext);
        }

        void IRecorderModule.Start(RecordContext recordContext, RecorderContext recorderContext)
        {
            if (IsRecording)
                throw new InvalidOperationException("Recorder module is already recording.");
            
            IsRecording = true;
            OnStart(recordContext, recorderContext);
        }

        async UniTask IRecorderModule.Stop(RecordContext recordContext, RecorderContext recorderContext)
        {
            EnsureIsRecording();
            await OnStop(recordContext, recorderContext);
            IsRecording = false;
        }

        void IRecorderModule.Reset(RecorderContext recorderContext)
        {
            _recordedObjects.Clear();
            _createdObjects.Clear();
            _destroyedObjects.Clear();
            OnReset(recorderContext);
        }

        protected void EnsureIsRecording()
        {
            if (!IsRecording)
                throw new InvalidOperationException("Recorder module is not recording.");
        }

        protected virtual void OnCreate(RecorderContext recorderContext)
        {
        }

        protected virtual void OnDestroy(RecorderContext recorderContext)
        {
        }

        protected virtual void OnStart(RecordContext recordContext, RecorderContext recorderContext)
        {
        }

        protected virtual UniTask OnStop(RecordContext recordContext, RecorderContext recorderContext)
        {
            return UniTask.CompletedTask;
        }

        protected virtual void OnReset(RecorderContext recorderContext)
        {
        }

        protected virtual void OnStartRecordingObject(ObjectSafeRef<TObject> objSafeRef, bool markCreated)
        {
        }

        protected virtual void OnStopRecordingObject(ObjectSafeRef<TObject> objSafeRef, bool markDestroyed)
        {
        }

        protected virtual void OnMarkedCreated(ObjectSafeRef<TObject> objSafeRef)
        {
        }

        protected virtual void OnMarkedDestroyed(ObjectSafeRef<TObject> objSafeRef)
        {
        }
    }
}