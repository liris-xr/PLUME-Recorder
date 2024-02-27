using System;
using System.Collections.Generic;
using PLUME.Core.Object;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module;
using PLUME.Core.Recorder.Module.Frame;
using Unity.Collections;
using Object = UnityEngine.Object;

namespace PLUME.Base.Module.Unity
{
    public abstract class ObjectRecorderModule<TObject, TFrameData> : IObjectRecorderModule, IFrameDataRecorderModule
        where TObject : Object where TFrameData : IFrameData
    {
        public Type SupportedObjectType => typeof(TObject);

        public bool IsRecording { get; private set; }

        private readonly Dictionary<FrameInfo, TFrameData> _framesData = new(FrameInfoComparer.Instance);

        private readonly List<ObjectSafeRef<TObject>> _recordedObjects = new();
        private NativeHashSet<ObjectIdentifier> _recordedObjectsIdentifier;
        private NativeHashSet<ObjectIdentifier> _createdObjectsIdentifier;
        private NativeHashSet<ObjectIdentifier> _destroyedObjectsIdentifier;

        protected IReadOnlyList<ObjectSafeRef<TObject>> RecordedObjects => _recordedObjects;

        protected NativeHashSet<ObjectIdentifier>.ReadOnly RecordedObjectsIdentifier =>
            _recordedObjectsIdentifier.AsReadOnly();

        protected NativeHashSet<ObjectIdentifier>.ReadOnly CreatedObjectsIdentifier =>
            _createdObjectsIdentifier.AsReadOnly();

        protected NativeHashSet<ObjectIdentifier>.ReadOnly DestroyedObjectsIdentifier =>
            _destroyedObjectsIdentifier.AsReadOnly();

        /// <summary>
        /// List of objects to remove from the recorded objects list after frame data collection.
        /// </summary>
        private readonly List<ObjectSafeRef<TObject>> _objectsToRemove = new();

        void IRecorderModule.Create(RecorderContext recorderContext)
        {
            _recordedObjectsIdentifier = new NativeHashSet<ObjectIdentifier>(1000, Allocator.Persistent);
            _createdObjectsIdentifier = new NativeHashSet<ObjectIdentifier>(1000, Allocator.Persistent);
            _destroyedObjectsIdentifier = new NativeHashSet<ObjectIdentifier>(1000, Allocator.Persistent);
            OnCreate(recorderContext);
        }

        void IRecorderModule.Destroy(RecorderContext recorderContext)
        {
            OnDestroy(recorderContext);
            _recordedObjectsIdentifier.Dispose();
            _createdObjectsIdentifier.Dispose();
            _destroyedObjectsIdentifier.Dispose();
        }

        public void StartRecordingObject(ObjectSafeRef<TObject> objSafeRef, bool markCreated, Record record,
            RecorderContext ctx)
        {
            CheckIsRecording();

            if (!_recordedObjectsIdentifier.Add(objSafeRef.Identifier))
                throw new InvalidOperationException("Object is already being recorded.");

            _recordedObjects.Add(objSafeRef);
            OnStartRecordingObject(objSafeRef, record, ctx);

            if (!markCreated)
                return;

            var markedCreated = _createdObjectsIdentifier.Add(objSafeRef.Identifier);
            _destroyedObjectsIdentifier.Remove(objSafeRef.Identifier);

            if (markedCreated)
                OnObjectMarkedCreated(objSafeRef);
        }

        public void StopRecordingObject(ObjectSafeRef<TObject> objSafeRef, bool markDestroyed, Record record,
            RecorderContext ctx)
        {
            CheckIsRecording();

            if (!_recordedObjectsIdentifier.Contains(objSafeRef.Identifier))
                throw new InvalidOperationException("Object is not being recorded.");

            // Deferred removal (after frame data collection)
            _objectsToRemove.Add(objSafeRef);

            if (markDestroyed)
            {
                var markedDestroyed = _destroyedObjectsIdentifier.Add(objSafeRef.Identifier);
                _createdObjectsIdentifier.Remove(objSafeRef.Identifier);

                if (markedDestroyed)
                    OnObjectMarkedDestroyed(objSafeRef);
            }
        }

        public void StartRecordingObject(IObjectSafeRef objSafeRef, bool markCreated, Record record,
            RecorderContext ctx)
        {
            if (objSafeRef is not ObjectSafeRef<TObject> tObjSafeRef)
            {
                throw new InvalidOperationException($"Object is not of type {typeof(TObject).Name}");
            }

            StartRecordingObject(tObjSafeRef, markCreated, record, ctx);
        }

        public void StopRecordingObject(IObjectSafeRef objSafeRef, bool markDestroyed, Record record,
            RecorderContext ctx)
        {
            if (objSafeRef is not ObjectSafeRef<TObject> tObjSafeRef)
            {
                throw new InvalidOperationException($"Object is not of type {typeof(TObject).Name}");
            }

            StopRecordingObject(tObjSafeRef, markDestroyed, record, ctx);
        }

        public bool IsObjectSupported(IObjectSafeRef objSafeRef)
        {
            return objSafeRef is ObjectSafeRef<TObject>;
        }

        void IFrameDataRecorderModule.EnqueueFrameData(FrameInfo frameInfo, Record record, RecorderContext context)
        {
            var frameData = CollectFrameData(frameInfo, record, context);

            lock (_framesData)
            {
                _framesData.Add(frameInfo, frameData);
            }
        }

        void IFrameDataRecorderModule.PostEnqueueFrameData(Record record, RecorderContext context)
        {
            _createdObjectsIdentifier.Clear();
            _destroyedObjectsIdentifier.Clear();

            foreach (var toRemove in _objectsToRemove)
            {
                _recordedObjectsIdentifier.Remove(toRemove.Identifier);
                _recordedObjects.Remove(toRemove);
                OnStopRecordingObject(toRemove, record, context);
            }

            _objectsToRemove.Clear();
        }

        bool IFrameDataRecorderModule.SerializeFrameData(FrameInfo frameInfo, FrameDataWriter frameDataWriter)
        {
            TFrameData frameData;

            lock (_framesData)
            {
                if (!_framesData.TryGetValue(frameInfo, out frameData))
                {
                    return false;
                }
            }

            frameData.Serialize(frameDataWriter);

            if (frameData is IDisposable disposable)
            {
                disposable.Dispose();
            }

            return true;
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
            _createdObjectsIdentifier.Clear();
            _destroyedObjectsIdentifier.Clear();

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

        protected virtual void OnStartRecordingObject(ObjectSafeRef<TObject> objSafeRef, Record record,
            RecorderContext ctx)
        {
        }

        protected virtual void OnStopRecordingObject(ObjectSafeRef<TObject> objSafeRef, Record record,
            RecorderContext recorderContext)
        {
        }

        protected virtual void OnObjectMarkedCreated(ObjectSafeRef<TObject> objSafeRef)
        {
        }

        protected virtual void OnObjectMarkedDestroyed(ObjectSafeRef<TObject> objSafeRef)
        {
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        void IFrameDataRecorderModule.FixedUpdate(long fixedDeltaTime, Record record, RecorderContext context)
        {
            OnFixedUpdate(fixedDeltaTime, record, context);
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        void IFrameDataRecorderModule.EarlyUpdate(long deltaTime, Record record, RecorderContext context)
        {
            OnEarlyUpdate(deltaTime, record, context);
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        void IFrameDataRecorderModule.PreUpdate(long deltaTime, Record record, RecorderContext context)
        {
            OnPreUpdate(deltaTime, record, context);
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        void IFrameDataRecorderModule.Update(long deltaTime, Record record, RecorderContext context)
        {
            OnUpdate(deltaTime, record, context);
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        void IFrameDataRecorderModule.PreLateUpdate(long deltaTime, Record record, RecorderContext context)
        {
            OnPreLateUpdate(deltaTime, record, context);
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        void IFrameDataRecorderModule.PostLateUpdate(long deltaTime, Record record, RecorderContext context)
        {
            OnPostLateUpdate(deltaTime, record, context);
        }

        protected abstract TFrameData CollectFrameData(FrameInfo frameInfo, Record record, RecorderContext ctx);

        // ReSharper restore Unity.PerformanceCriticalContext

        protected virtual void OnFixedUpdate(long fixedDeltaTime, Record record, RecorderContext context)
        {
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        protected virtual void OnEarlyUpdate(long deltaTime, Record record, RecorderContext context)
        {
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        protected virtual void OnPreUpdate(long deltaTime, Record record, RecorderContext context)
        {
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        protected virtual void OnUpdate(long deltaTime, Record record, RecorderContext context)
        {
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        protected virtual void OnPreLateUpdate(long deltaTime, Record record, RecorderContext context)
        {
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        protected virtual void OnPostLateUpdate(long deltaTime, Record record, RecorderContext context)
        {
        }
    }
}