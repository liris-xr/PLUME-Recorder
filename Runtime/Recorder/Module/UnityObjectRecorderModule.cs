using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PLUME.Recorder.Module
{
    public abstract class UnityObjectRecorderModule<TObject> : IUnityObjectRecorderModule where TObject : Object
    {
        private bool _started;

        private readonly UnityObjectCollection<TObject> _recordedObjects = new();
        private readonly UnityObjectCollection<TObject> _createdObjects = new();
        private readonly UnityObjectCollection<TObject> _destroyedObjects = new();

        protected IReadOnlyList<ObjectSafeRef<TObject>> RecordedObjects => _recordedObjects.AsReadOnly();
        protected IReadOnlyList<ObjectSafeRef<TObject>> CreatedObjects => _createdObjects.AsReadOnly();
        protected IReadOnlyList<ObjectSafeRef<TObject>> DestroyedObjects => _destroyedObjects.AsReadOnly();

        public bool TryStartRecordingObject(ObjectSafeRef<TObject> objectSafeRef, bool markCreated)
        {
            if (!_started)
                return false;

            if (!_recordedObjects.TryAdd(objectSafeRef))
                return false;

            OnStartRecording(objectSafeRef, markCreated);

            if (!markCreated)
                return true;

            var successfullyMarkedCreated = _createdObjects.TryAdd(objectSafeRef);
            _destroyedObjects.TryRemove(objectSafeRef);

            if (successfullyMarkedCreated)
                OnMarkedCreated(objectSafeRef);

            return true;
        }

        public bool TryStopRecordingObject(ObjectSafeRef<TObject> objSafeRef, bool markDestroyed)
        {
            if (!_started)
                return false;

            if (!_recordedObjects.TryRemove(objSafeRef))
                return false;

            OnStopRecording(objSafeRef, markDestroyed);

            if (!markDestroyed)
                return true;

            var successfullyMarkedDestroyed = _destroyedObjects.TryRemove(objSafeRef);
            _createdObjects.TryRemove(objSafeRef);

            if (successfullyMarkedDestroyed)
                OnMarkedDestroyed(objSafeRef);
            return true;
        }
        
        protected void ClearCreatedObjects()
        {
            _createdObjects.Clear();
        }
        
        protected void ClearDestroyedObjects()
        {
            _destroyedObjects.Clear();
        }
        
        public bool TryStartRecordingObject(IObjectSafeRef objSafeRef, bool markCreated)
        {
            return objSafeRef is ObjectSafeRef<TObject> tObjSafeRef && TryStartRecordingObject(tObjSafeRef, markCreated);
        }
        
        public bool TryStopRecordingObject(IObjectSafeRef objSafeRef, bool markCreated)
        {
            return objSafeRef is ObjectSafeRef<TObject> tObjSafeRef && TryStopRecordingObject(tObjSafeRef, markCreated);
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
            _started = true;
            OnStart();
        }

        void IRecorderModule.Stop()
        {
            _started = false;
            OnStop();
        }

        void IRecorderModule.Reset()
        {
            _recordedObjects.Clear();
            _createdObjects.Clear();
            _destroyedObjects.Clear();
            OnReset();
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