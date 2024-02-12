using System;
using System.Collections.Generic;
using PLUME.Core.Object;
using PLUME.Core.Object.SafeRef;

namespace PLUME.Core.Recorder.Module
{
    public abstract class ObjectRecorderModule<TObject> : IObjectRecorderModule where TObject : UnityEngine.Object
    {
        public Type SupportedObjectType => typeof(TObject);
        
        private bool _isRecording;

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

            OnStartRecording(objectSafeRef, markCreated);

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

            OnStopRecording(objSafeRef, markDestroyed);

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

        void IRecorderModule.Create()
        {
            OnCreate();
        }

        void IRecorderModule.Destroy()
        {
            OnDestroy();
        }

        void IRecorderModule.Start()
        {
            _isRecording = true;
            OnStart();
        }

        void IRecorderModule.Stop()
        {
            OnStop();
            _isRecording = false;
        }

        void IRecorderModule.Reset()
        {
            _recordedObjects.Clear();
            _createdObjects.Clear();
            _destroyedObjects.Clear();
            OnReset();
        }

        private void EnsureIsRecording()
        {
            if (!_isRecording)
                throw new InvalidOperationException("Recorder module is not recording.");
        }

        protected virtual void OnCreate()
        {
        }

        protected virtual void OnDestroy()
        {
        }

        protected virtual void OnStart()
        {
        }

        protected virtual void OnStop()
        {
        }

        protected virtual void OnReset()
        {
        }

        protected virtual void OnStartRecording(ObjectSafeRef<TObject> objSafeRef, bool markCreated)
        {
        }

        protected virtual void OnStopRecording(ObjectSafeRef<TObject> objSafeRef, bool markDestroyed)
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