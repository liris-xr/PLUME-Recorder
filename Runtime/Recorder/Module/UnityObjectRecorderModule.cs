using System.Collections.Generic;
using UnityEngine;

namespace PLUME.Recorder.Module
{
    public abstract class UnityObjectRecorderModule<TObject> : IUnityObjectRecorderModule where TObject : Object
    {
        private bool _running;

        private readonly HashSet<ObjectSafeRef<TObject>> _recordedObjects = new(ObjectSafeRef<TObject>.Comparer);
        private readonly HashSet<ObjectSafeRef<TObject>> _createdObjects = new(ObjectSafeRef<TObject>.Comparer);
        private readonly HashSet<ObjectSafeRef<TObject>> _destroyedObjects = new(ObjectSafeRef<TObject>.Comparer);

        protected IReadOnlyCollection<ObjectSafeRef<TObject>> RecordedObjects => _recordedObjects;
        protected IReadOnlyCollection<ObjectSafeRef<TObject>> CreatedObjects => _createdObjects;
        protected IReadOnlyCollection<ObjectSafeRef<TObject>> DestroyedObjects => _destroyedObjects;

        void IUnityRecorderModule.RecordFrame(FrameData frameData)
        {
            OnRecordFrame(frameData);
            _createdObjects.Clear();
            _destroyedObjects.Clear();
        }

        public bool TryStartRecording(IObjectSafeRef objSafeRef, bool markCreated)
        {
            if (!_running)
                return false;

            if (objSafeRef is not ObjectSafeRef<TObject> typedObjSafeRef)
                return false;

            if (!_recordedObjects.Add(typedObjSafeRef))
                return false;

            if (markCreated)
            {
                _destroyedObjects.Remove(typedObjSafeRef);
                _createdObjects.Add(typedObjSafeRef);
                OnMarkedCreated(typedObjSafeRef);
            }

            OnStartRecording(typedObjSafeRef, markCreated);
            return true;
        }

        public bool TryStopRecording(IObjectSafeRef objSafeRef, bool markDestroyed)
        {
            if (!_running)
                return false;

            if (objSafeRef is not ObjectSafeRef<TObject> typedObjSafeRef)
                return false;

            if (!_recordedObjects.Remove(typedObjSafeRef))
                return false;

            if (markDestroyed)
            {
                _createdObjects.Remove(typedObjSafeRef);
                _destroyedObjects.Add(typedObjSafeRef);
                _recordedObjects.Remove(typedObjSafeRef);
                OnMarkedDestroyed(typedObjSafeRef);
            }

            OnStopRecording(typedObjSafeRef, markDestroyed);
            return true;
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
            _running = true;
            OnStart();
        }

        void IRecorderModule.Stop()
        {
            _running = false;
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

        protected virtual void OnRecordFrame(FrameData frameData)
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