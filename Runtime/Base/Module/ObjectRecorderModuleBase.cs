using System;
using System.Collections.Generic;
using PLUME.Core.Object;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module;
using Unity.Collections;
using Object = UnityEngine.Object;

namespace PLUME.Base.Module
{
    public abstract class ObjectRecorderModuleBase<TObject> : IObjectRecorderModule where TObject : Object
    {
        public Type SupportedObjectType => typeof(TObject);

        public bool IsRecording { get; private set; }

        private ObjectCollection<TObject> _recordedObjects;

        protected IReadOnlyList<ObjectSafeRef<TObject>> RecordedObjects => _recordedObjects.AsReadOnly();

        void IRecorderModule.Create(RecorderContext recorderContext)
        {
            _recordedObjects = new ObjectCollection<TObject>(1000, Allocator.Persistent);
            OnCreate(recorderContext);
        }

        void IRecorderModule.Destroy(RecorderContext recorderContext)
        {
            OnDestroy(recorderContext);
            _recordedObjects.Dispose();
        }
        
        public void StartRecordingObject(ObjectSafeRef<TObject> objectSafeRef, bool markCreated, Record record,
            RecorderContext recorderContext)
        {
            CheckIsRecording();

            if (!_recordedObjects.TryAdd(objectSafeRef))
                throw new InvalidOperationException("Object is already being recorded.");

            OnStartRecordingObject(objectSafeRef, markCreated, record, recorderContext);
        }

        public void StopRecordingObject(ObjectSafeRef<TObject> objSafeRef, bool markDestroyed, Record record,
            RecorderContext recorderContext)
        {
            CheckIsRecording();

            if (!_recordedObjects.TryRemove(objSafeRef))
                throw new InvalidOperationException("Object is not being recorded.");

            OnStopRecordingObject(objSafeRef, markDestroyed, record, recorderContext);
        }

        public void StartRecordingObject(IObjectSafeRef objSafeRef, bool markCreated, Record record,
            RecorderContext recorderContext)
        {
            if (objSafeRef is not ObjectSafeRef<TObject> tObjSafeRef)
            {
                throw new InvalidOperationException($"Object is not of type {typeof(TObject).Name}");
            }

            StartRecordingObject(tObjSafeRef, markCreated, record, recorderContext);
        }

        public void StopRecordingObject(IObjectSafeRef objSafeRef, bool markDestroyed, Record record,
            RecorderContext recorderContext)
        {
            if (objSafeRef is not ObjectSafeRef<TObject> tObjSafeRef)
            {
                throw new InvalidOperationException($"Object is not of type {typeof(TObject).Name}");
            }

            StopRecordingObject(tObjSafeRef, markDestroyed, record, recorderContext);
        }

        public bool IsRecordingObject(IObjectSafeRef objSafeRef)
        {
            return objSafeRef is ObjectSafeRef<TObject> tObjSafeRef && _recordedObjects.Contains(tObjSafeRef);
        }

        void IRecorderModule.Awake(RecorderContext context)
        {
            OnAwake(context);
        }

        void IRecorderModule.StartRecording(Record record, RecorderContext recorderContext)
        {
            if (IsRecording)
                throw new InvalidOperationException("Recorder module is already recording.");

            IsRecording = true;
            OnStartRecording(record, recorderContext);
        }

        void IRecorderModule.StopRecording(Record record, RecorderContext recorderContext)
        {
            CheckIsRecording();
            OnStopRecording(record, recorderContext);

            _recordedObjects.Clear();
            IsRecording = false;
        }

        protected void CheckIsRecording()
        {
            if (!IsRecording)
                throw new InvalidOperationException("Recorder module is not recording.");
        }

        protected virtual void OnAwake(RecorderContext context)
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

        protected virtual void OnStopRecording(Record record, RecorderContext recorderContext)
        {
        }

        protected virtual void OnStartRecordingObject(ObjectSafeRef<TObject> objSafeRef, bool markCreated,
            Record record, RecorderContext recorderContext)
        {
        }

        protected virtual void OnStopRecordingObject(ObjectSafeRef<TObject> objSafeRef, bool markDestroyed,
            Record record, RecorderContext recorderContext)
        {
        }
    }
}