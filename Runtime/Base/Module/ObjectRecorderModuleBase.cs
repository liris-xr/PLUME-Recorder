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

        private readonly List<ObjectSafeRef<TObject>> _recordedObjects = new();
        private NativeHashSet<ObjectIdentifier> _recordedObjectsIdentifier;

        protected IReadOnlyList<ObjectSafeRef<TObject>> RecordedObjects => _recordedObjects.AsReadOnly();

        void IRecorderModule.Create(RecorderContext recorderContext)
        {
            _recordedObjectsIdentifier = new NativeHashSet<ObjectIdentifier>(1000, Allocator.Persistent);
            OnCreate(recorderContext);
        }

        void IRecorderModule.Destroy(RecorderContext recorderContext)
        {
            OnDestroy(recorderContext);
            _recordedObjectsIdentifier.Dispose();
        }
        
        public void StartRecordingObject(ObjectSafeRef<TObject> objSafeRef, bool markCreated, Record record,
            RecorderContext recorderContext)
        {
            CheckIsRecording();

            if(!_recordedObjectsIdentifier.Add(objSafeRef.Identifier))
                throw new InvalidOperationException("Object is already being recorded.");

            _recordedObjects.Add(objSafeRef);
            OnStartRecordingObject(objSafeRef, markCreated, record, recorderContext);
        }

        public void StopRecordingObject(ObjectSafeRef<TObject> objSafeRef, bool markDestroyed, Record record,
            RecorderContext recorderContext)
        {
            CheckIsRecording();

            if(!_recordedObjectsIdentifier.Remove(objSafeRef.Identifier))
                throw new InvalidOperationException("Object is not being recorded.");
            
            _recordedObjects.Remove(objSafeRef);
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