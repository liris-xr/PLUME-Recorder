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
    public abstract class ObjectRecorderModule<TObject, TObjectIdentifier, TObjectSafeRef, TFrameData> :
        IObjectRecorderModule, IFrameDataRecorderModule
        where TObject : Object
        where TObjectIdentifier : unmanaged, IObjectIdentifier, IEquatable<TObjectIdentifier>
        where TObjectSafeRef : IObjectSafeRef<TObject, TObjectIdentifier>
        where TFrameData : IFrameData
    {
        public bool IsRecording { get; private set; }

        private readonly Dictionary<FrameInfo, TFrameData> _framesData = new(FrameInfoComparer.Instance);

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

        void IRecorderModule.Create(RecorderContext recorderContext)
        {
            _recordedObjectsIdentifier = new NativeHashSet<TObjectIdentifier>(1000, Allocator.Persistent);
            _createdObjectsIdentifier = new NativeHashSet<TObjectIdentifier>(1000, Allocator.Persistent);
            _destroyedObjectsIdentifier = new NativeHashSet<TObjectIdentifier>(1000, Allocator.Persistent);
            OnCreate(recorderContext);
        }

        void IRecorderModule.Destroy(RecorderContext recorderContext)
        {
            OnDestroy(recorderContext);
            _recordedObjectsIdentifier.Dispose();
            _createdObjectsIdentifier.Dispose();
            _destroyedObjectsIdentifier.Dispose();
        }

        public void StartRecordingObject(TObjectSafeRef objSafeRef, bool markCreated,
            Record record,
            RecorderContext ctx)
        {
            CheckIsRecording();

            if (!_recordedObjectsIdentifier.Add(objSafeRef.GetIdentifier()))
                throw new InvalidOperationException("Object is already being recorded.");

            _recordedObjects.Add(objSafeRef);
            OnStartRecordingObject(objSafeRef, record, ctx);

            if (!markCreated)
                return;

            var markedCreated = _createdObjectsIdentifier.Add(objSafeRef.GetIdentifier());
            _destroyedObjectsIdentifier.Remove(objSafeRef.GetIdentifier());

            if (markedCreated)
                OnObjectMarkedCreated(objSafeRef);
        }

        public void StopRecordingObject(TObjectSafeRef objSafeRef, bool markDestroyed,
            Record record,
            RecorderContext ctx)
        {
            CheckIsRecording();

            if (!_recordedObjectsIdentifier.Contains(objSafeRef.GetIdentifier()))
                throw new InvalidOperationException("Object is not being recorded.");

            // Deferred removal (after frame data collection)
            _objectsToRemove.Add(objSafeRef);

            if (markDestroyed)
            {
                var markedDestroyed = _destroyedObjectsIdentifier.Add(objSafeRef.GetIdentifier());
                _createdObjectsIdentifier.Remove(objSafeRef.GetIdentifier());

                if (markedDestroyed)
                    OnObjectMarkedDestroyed(objSafeRef);
            }
        }

        public void StartRecordingObject(IObjectSafeRef objSafeRef, bool markCreated, Record record,
            RecorderContext ctx)
        {
            if (objSafeRef is not TObjectSafeRef tObjSafeRef)
            {
                throw new InvalidOperationException($"Object is not of type {typeof(TObject).Name}");
            }

            StartRecordingObject(tObjSafeRef, markCreated, record, ctx);
        }

        public void StopRecordingObject(IObjectSafeRef objSafeRef, bool markDestroyed, Record record,
            RecorderContext ctx)
        {
            if (objSafeRef is not TObjectSafeRef tObjSafeRef)
            {
                throw new InvalidOperationException($"Object is not of type {typeof(TObject).Name}");
            }

            StopRecordingObject(tObjSafeRef, markDestroyed, record, ctx);
        }

        public bool IsObjectSupported(IObjectSafeRef objSafeRef)
        {
            return objSafeRef is TObjectSafeRef;
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
                _recordedObjectsIdentifier.Remove(toRemove.GetIdentifier());
                _recordedObjects.Remove(toRemove);
                OnStopRecordingObject(toRemove, record, context);
            }

            _objectsToRemove.Clear();
        }

        void IFrameDataRecorderModule.SerializeFrameData(FrameInfo frameInfo, FrameDataWriter frameDataWriter)
        {
            TFrameData frameData;

            lock (_framesData)
            {
                if (!_framesData.TryGetValue(frameInfo, out frameData))
                {
                    return;
                }
            }

            frameData.Serialize(frameDataWriter);

            if (frameData is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        public bool IsRecordingObject(IObjectSafeRef objSafeRef)
        {
            return objSafeRef is TObjectSafeRef tObjSafeRef &&
                   _recordedObjects.Contains(tObjSafeRef);
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

        protected virtual void OnStartRecordingObject(TObjectSafeRef objSafeRef, Record record,
            RecorderContext ctx)
        {
        }

        protected virtual void OnStopRecordingObject(TObjectSafeRef objSafeRef, Record record,
            RecorderContext recorderContext)
        {
        }

        protected virtual void OnObjectMarkedCreated(TObjectSafeRef objSafeRef)
        {
        }

        protected virtual void OnObjectMarkedDestroyed(TObjectSafeRef objSafeRef)
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