using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PLUME.Core.Object;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module;

namespace PLUME.Base.Module
{
    public abstract class ObjectRecorderModuleBase<TObject> : IObjectRecorderModule where TObject : UnityEngine.Object
    {
        public Type SupportedObjectType => typeof(TObject);

        private bool _recording;

        private readonly ObjectCollection<TObject> _recordedObjects = new();
        private readonly ObjectCollection<TObject> _createdObjects = new();
        private readonly ObjectCollection<TObject> _destroyedObjects = new();

        protected IReadOnlyList<ObjectSafeRef<TObject>> RecordedObjects => _recordedObjects.AsReadOnly();
        protected IReadOnlyList<ObjectSafeRef<TObject>> CreatedObjects => _createdObjects.AsReadOnly();
        protected IReadOnlyList<ObjectSafeRef<TObject>> DestroyedObjects => _destroyedObjects.AsReadOnly();

        public void StartRecordingObject(ObjectSafeRef<TObject> objectSafeRef, bool markCreated)
        {
            CheckIsRecording();

            if (!_recordedObjects.TryAdd(objectSafeRef))
                throw new InvalidOperationException("Object is already being recorded.");

            OnStartRecordingObject(objectSafeRef, markCreated);

            if (!markCreated)
                return;

            var successfullyMarkedCreated = _createdObjects.TryAdd(objectSafeRef);
            _destroyedObjects.TryRemove(objectSafeRef);

            if (successfullyMarkedCreated)
                OnObjectMarkedCreated(objectSafeRef);
        }

        public void StopRecordingObject(ObjectSafeRef<TObject> objSafeRef, bool markDestroyed)
        {
            CheckIsRecording();

            if (!_recordedObjects.TryRemove(objSafeRef))
                throw new InvalidOperationException("Object is not being recorded.");

            OnStopRecordingObject(objSafeRef, markDestroyed);

            if (!markDestroyed)
                return;

            var successfullyMarkedDestroyed = _destroyedObjects.TryRemove(objSafeRef);
            _createdObjects.TryRemove(objSafeRef);

            if (successfullyMarkedDestroyed)
                OnObjectMarkedDestroyed(objSafeRef);
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

        void IRecorderModule.StartRecording(Record record, RecorderContext recorderContext)
        {
            if (_recording)
                throw new InvalidOperationException("Recorder module is already recording.");
            
            _recording = true;
            OnStartRecording(record, recorderContext);
        }

        void IRecorderModule.ForceStopRecording(Record record, RecorderContext recorderContext)
        {
            CheckIsRecording();
            OnForceStopRecording(record, recorderContext);
            
            _recordedObjects.Clear();
            _createdObjects.Clear();
            _destroyedObjects.Clear();
            
            _recording = false;
        }
        
        async UniTask IRecorderModule.StopRecording(Record record, RecorderContext recorderContext)
        {
            CheckIsRecording();
            await OnStopRecording(record, recorderContext);
            
            _recordedObjects.Clear();
            _createdObjects.Clear();
            _destroyedObjects.Clear();
            
            _recording = false;
        }

        protected void CheckIsRecording()
        {
            if (!_recording)
                throw new InvalidOperationException("Recorder module is not recording.");
        }

        void IRecorderModule.EarlyUpdate(Record record, RecorderContext context)
        {
            OnEarlyUpdate(record, context);
        }

        void IRecorderModule.PreUpdate(Record record, RecorderContext context)
        {
            OnPreUpdate(record, context);
        }

        void IRecorderModule.Update(Record record, RecorderContext context)
        {
        }

        void IRecorderModule.PreLateUpdate(Record record, RecorderContext context)
        {
            OnPreLateUpdate(record, context);
        }

        void IRecorderModule.PostLateUpdate(Record record, RecorderContext context)
        {
            OnPostLateUpdate(record, context);
        }
        
        bool IRecorderModule.IsRecording()
        {
            return _recording;
        }

        protected virtual void OnFixedUpdate(Record record, RecorderContext context)
        {
        }

        protected virtual void OnEarlyUpdate(Record record, RecorderContext context)
        {
        }

        protected virtual void OnPreUpdate(Record record, RecorderContext context)
        {
        }

        protected virtual void OnUpdate(Record record, RecorderContext context)
        {
        }

        protected virtual void OnPreLateUpdate(Record record, RecorderContext context)
        {
        }

        protected virtual void OnPostLateUpdate(Record record, RecorderContext context)
        {
        }
        
        protected virtual void OnCreate(RecorderContext recorderContext)
        {
        }

        protected virtual void OnDestroy(RecorderContext recorderContext)
        {
        }

        protected virtual void OnStartRecording(Record record, RecorderContext recorderContext)
        {
        }

        protected virtual void OnForceStopRecording(Record record, RecorderContext recorderContext)
        {
        }
        
        protected virtual UniTask OnStopRecording(Record record, RecorderContext recorderContext)
        {
            return UniTask.CompletedTask;
        }

        protected virtual void OnStartRecordingObject(ObjectSafeRef<TObject> objSafeRef, bool markCreated)
        {
        }

        protected virtual void OnStopRecordingObject(ObjectSafeRef<TObject> objSafeRef, bool markDestroyed)
        {
        }

        protected virtual void OnObjectMarkedCreated(ObjectSafeRef<TObject> objSafeRef)
        {
        }

        protected virtual void OnObjectMarkedDestroyed(ObjectSafeRef<TObject> objSafeRef)
        {
        }
    }
}