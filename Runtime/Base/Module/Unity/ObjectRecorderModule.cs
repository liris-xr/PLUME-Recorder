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
        where TObjectSafeRef : IObjectSafeRef<TObjectIdentifier>
        where TFrameData : IFrameData
    {
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

        void IRecorderModule.Create(RecorderContext ctx)
        {
            _recordedObjectsIdentifier = new NativeHashSet<TObjectIdentifier>(1000, Allocator.Persistent);
            _createdObjectsIdentifier = new NativeHashSet<TObjectIdentifier>(1000, Allocator.Persistent);
            _destroyedObjectsIdentifier = new NativeHashSet<TObjectIdentifier>(1000, Allocator.Persistent);
            OnCreate(ctx);
        }

        void IRecorderModule.Destroy(RecorderContext ctx)
        {
            OnDestroy(ctx);
            _recordedObjectsIdentifier.Dispose();
            _createdObjectsIdentifier.Dispose();
            _destroyedObjectsIdentifier.Dispose();
        }

        public void StartRecordingObject(TObjectSafeRef objSafeRef, bool markCreated, RecorderContext ctx)
        {
            CheckIsRecording(ctx);

            if (!_recordedObjectsIdentifier.Add(objSafeRef.Identifier))
                throw new InvalidOperationException("Object is already being recorded.");

            _recordedObjects.Add(objSafeRef);
            OnStartRecordingObject(objSafeRef, ctx);

            if (!markCreated)
                return;

            var markedCreated = _createdObjectsIdentifier.Add(objSafeRef.Identifier);
            _destroyedObjectsIdentifier.Remove(objSafeRef.Identifier);

            if (markedCreated)
                OnObjectMarkedCreated(objSafeRef, ctx);
        }

        public void StopRecordingObject(TObjectSafeRef objSafeRef, bool markDestroyed, RecorderContext ctx)
        {
            CheckIsRecording(ctx);

            if (!_recordedObjectsIdentifier.Contains(objSafeRef.Identifier))
                throw new InvalidOperationException("Object is not being recorded.");

            // Deferred removal (after frame data collection)
            _objectsToRemove.Add(objSafeRef);

            if (markDestroyed)
            {
                var markedDestroyed = _destroyedObjectsIdentifier.Add(objSafeRef.Identifier);
                _createdObjectsIdentifier.Remove(objSafeRef.Identifier);

                if (markedDestroyed)
                    OnObjectMarkedDestroyed(objSafeRef, ctx);
            }
        }

        public void StartRecordingObject(IObjectSafeRef objSafeRef, bool markCreated, RecorderContext ctx)
        {
            CheckIsRecording(ctx);
            
            if (objSafeRef is not TObjectSafeRef tObjSafeRef)
            {
                throw new InvalidOperationException($"Object is not of type {typeof(TObject).Name}");
            }

            StartRecordingObject(tObjSafeRef, markCreated, ctx);
        }

        public void StopRecordingObject(IObjectSafeRef objSafeRef, bool markDestroyed, RecorderContext ctx)
        {
            CheckIsRecording(ctx);
            
            if (objSafeRef is not TObjectSafeRef tObjSafeRef)
            {
                throw new InvalidOperationException($"Object is not of type {typeof(TObject).Name}");
            }

            StopRecordingObject(tObjSafeRef, markDestroyed, ctx);
        }

        public bool IsObjectSupported(IObjectSafeRef objSafeRef)
        {
            return objSafeRef is TObjectSafeRef;
        }

        void IFrameDataRecorderModule.EnqueueFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            OnBeforeCollectFrameData(frameInfo, ctx);
            var frameData = CollectFrameData(frameInfo, ctx);
            OnAfterCollectFrameData(frameInfo, ctx);

            lock (_framesData)
            {
                _framesData.Add(frameInfo, frameData);
            }
        }

        void IFrameDataRecorderModule.PostEnqueueFrameData(RecorderContext ctx)
        {
            _createdObjectsIdentifier.Clear();
            _destroyedObjectsIdentifier.Clear();

            foreach (var toRemove in _objectsToRemove)
            {
                _recordedObjectsIdentifier.Remove(toRemove.Identifier);
                _recordedObjects.Remove(toRemove);
                OnStopRecordingObject(toRemove, ctx);
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
            return objSafeRef is TObjectSafeRef tObjSafeRef && _recordedObjects.Contains(tObjSafeRef);
        }

        void IRecorderModule.Awake(RecorderContext ctx)
        {
            OnAwake(ctx);
        }

        void IRecorderModule.StartRecording(RecorderContext ctx)
        {
            OnStartRecording(ctx);
        }

        void IRecorderModule.StopRecording(RecorderContext ctx)
        {
            OnStopRecording(ctx);
            _recordedObjects.Clear();
            _createdObjectsIdentifier.Clear();
            _destroyedObjectsIdentifier.Clear();
        }

        protected void CheckIsRecording(RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                throw new InvalidOperationException("Recorder module is not recording.");
        }

        protected virtual void OnAwake(RecorderContext ctx)
        {
        }

        protected virtual void OnCreate(RecorderContext ctx)
        {
        }

        protected virtual void OnDestroy(RecorderContext ctx)
        {
        }

        protected virtual void OnStartRecording(RecorderContext ctx)
        {
        }

        protected virtual void OnStopRecording(RecorderContext ctx)
        {
        }

        protected virtual void OnStartRecordingObject(TObjectSafeRef objSafeRef, RecorderContext ctx)
        {
        }

        protected virtual void OnStopRecordingObject(TObjectSafeRef objSafeRef, RecorderContext ctx)
        {
        }

        protected virtual void OnObjectMarkedCreated(TObjectSafeRef objSafeRef, RecorderContext ctx)
        {
        }

        protected virtual void OnObjectMarkedDestroyed(TObjectSafeRef objSafeRef, RecorderContext ctx)
        {
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        void IFrameDataRecorderModule.FixedUpdate(ulong fixedDeltaTime, RecorderContext context)
        {
            OnFixedUpdate(fixedDeltaTime, context);
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        void IFrameDataRecorderModule.EarlyUpdate(ulong deltaTime, RecorderContext ctx)
        {
            OnEarlyUpdate(deltaTime, ctx);
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        void IFrameDataRecorderModule.PreUpdate(ulong deltaTime, RecorderContext ctx)
        {
            OnPreUpdate(deltaTime, ctx);
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        void IFrameDataRecorderModule.Update(ulong deltaTime, RecorderContext ctx)
        {
            OnUpdate(deltaTime, ctx);
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        void IFrameDataRecorderModule.PreLateUpdate(ulong deltaTime, RecorderContext ctx)
        {
            OnPreLateUpdate(deltaTime, ctx);
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        void IFrameDataRecorderModule.PostLateUpdate(ulong deltaTime, RecorderContext ctx)
        {
            OnPostLateUpdate(deltaTime, ctx);
        }

        protected virtual void OnBeforeCollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
        }
        
        protected abstract TFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx);
        
        protected virtual void OnAfterCollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        protected virtual void OnFixedUpdate(ulong fixedDeltaTime, RecorderContext ctx)
        {
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        protected virtual void OnEarlyUpdate(ulong deltaTime, RecorderContext ctx)
        {
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        protected virtual void OnPreUpdate(ulong deltaTime, RecorderContext ctx)
        {
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        protected virtual void OnUpdate(ulong deltaTime, RecorderContext ctx)
        {
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        protected virtual void OnPreLateUpdate(ulong deltaTime, RecorderContext ctx)
        {
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        protected virtual void OnPostLateUpdate(ulong deltaTime, RecorderContext ctx)
        {
        }
    }
}