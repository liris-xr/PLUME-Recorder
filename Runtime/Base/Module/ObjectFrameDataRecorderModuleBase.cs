using System;
using System.Collections.Generic;
using PLUME.Core.Object;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module;
using PLUME.Core.Recorder.Module.Frame;
using Unity.Collections;
using Object = UnityEngine.Object;

namespace PLUME.Base.Module
{
    public abstract class ObjectFrameDataRecorderModuleBase<TObject, TFrameData> : IObjectRecorderModule,
        IFrameDataRecorderModule where TObject : Object where TFrameData : IFrameData
    {
        public Type SupportedObjectType => typeof(TObject);

        public bool IsRecording { get; private set; }

        private readonly Dictionary<FrameInfo, TFrameData> _framesData = new(FrameInfoComparer.Instance);

        private readonly List<ObjectSafeRef<TObject>> _toRemove = new();

        private ObjectCollection<TObject> _recordedObjects;
        private ObjectCollection<TObject> _createdInFrame;
        private ObjectCollection<TObject> _destroyedInFrame;

        protected IReadOnlyList<ObjectSafeRef<TObject>> RecordedObjects => _recordedObjects.AsReadOnly();

        protected NativeHashSet<ObjectIdentifier>.ReadOnly CreatedInFrame => _createdInFrame.GetIdentifiers();

        protected NativeHashSet<ObjectIdentifier>.ReadOnly DestroyedInFrame => _destroyedInFrame.GetIdentifiers();

        void IRecorderModule.Create(RecorderContext recorderContext)
        {
            _recordedObjects = new ObjectCollection<TObject>(1000, Allocator.Persistent);
            _createdInFrame = new ObjectCollection<TObject>(1000, Allocator.Persistent);
            _destroyedInFrame = new ObjectCollection<TObject>(1000, Allocator.Persistent);
            OnCreate(recorderContext);
        }

        void IRecorderModule.Destroy(RecorderContext recorderContext)
        {
            OnDestroy(recorderContext);
            _recordedObjects.Dispose();
            _createdInFrame.Dispose();
            _destroyedInFrame.Dispose();
        }

        public void StartRecordingObject(ObjectSafeRef<TObject> objSafeRef, bool markCreated, Record record,
            RecorderContext recorderContext)
        {
            CheckIsRecording();

            if (!_recordedObjects.TryAdd(objSafeRef))
                throw new InvalidOperationException("Object is already being recorded.");

            OnStartRecordingObject(objSafeRef, record, recorderContext);

            if (!markCreated)
                return;

            var markedCreated = _createdInFrame.TryAdd(objSafeRef);
            _destroyedInFrame.TryRemove(objSafeRef);

            if (markedCreated)
                OnObjectMarkedCreated(objSafeRef);
        }

        public void StopRecordingObject(ObjectSafeRef<TObject> objSafeRef, bool markDestroyed, Record record,
            RecorderContext recorderContext)
        {
            CheckIsRecording();

            if(!_recordedObjects.Contains(objSafeRef))
                throw new InvalidOperationException("Object is not being recorded.");
            
            // Deferred removal (after frame data collection)
            _toRemove.Add(objSafeRef);
            
            if (markDestroyed)
            {
                var markedDestroyed = _destroyedInFrame.TryAdd(objSafeRef);
                _createdInFrame.TryRemove(objSafeRef);

                if (markedDestroyed)
                    OnObjectMarkedDestroyed(objSafeRef);
            }
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
            _createdInFrame.Clear();
            _destroyedInFrame.Clear();

            foreach (var toRemove in _toRemove)
            {
                if (_recordedObjects.TryRemove(toRemove))
                {
                    OnStopRecordingObject(toRemove, record, context);
                }
            }
            
            _toRemove.Clear();
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
            _createdInFrame.Clear();
            _destroyedInFrame.Clear();

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
            RecorderContext recorderContext)
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

        protected abstract TFrameData CollectFrameData(FrameInfo frameInfo, Record record, RecorderContext context);

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