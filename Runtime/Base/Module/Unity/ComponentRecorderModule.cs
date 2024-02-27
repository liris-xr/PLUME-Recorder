using System;
using System.Collections.Generic;
using PLUME.Core.Object;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module;
using PLUME.Core.Recorder.Module.Frame;
using Unity.Collections;
using UnityEngine;

namespace PLUME.Base.Module.Unity
{
    public abstract class ComponentRecorderModule<TC, TD> : IObjectRecorderModule, IFrameDataRecorderModule
        where TC : Component where TD : IFrameData
    {
        public bool IsRecording { get; private set; }

        private readonly Dictionary<FrameInfo, TD> _framesData = new(FrameInfoComparer.Instance);

        private readonly List<ComponentSafeRef<TC>> _recordedComponents = new();
        private NativeHashSet<ComponentIdentifier> _recordedComponentsIdentifier;
        private NativeHashSet<ComponentIdentifier> _createdComponentsIdentifier;
        private NativeHashSet<ComponentIdentifier> _destroyedComponentsIdentifier;

        protected IReadOnlyList<ComponentSafeRef<TC>> RecordedComponents => _recordedComponents;
        protected NativeHashSet<ComponentIdentifier>.ReadOnly RecordedComponentsIdentifier => _recordedComponentsIdentifier.AsReadOnly();
        protected NativeHashSet<ComponentIdentifier>.ReadOnly CreatedComponentsIdentifier => _createdComponentsIdentifier.AsReadOnly();
        protected NativeHashSet<ComponentIdentifier>.ReadOnly DestroyedComponentsIdentifier => _destroyedComponentsIdentifier.AsReadOnly();

        /// <summary>
        /// List of objects to remove from the recorded objects list after frame data collection.
        /// </summary>
        private readonly List<ComponentSafeRef<TC>> _componentsToRemove = new();

        void IRecorderModule.Create(RecorderContext recorderContext)
        {
            _recordedComponentsIdentifier = new NativeHashSet<ComponentIdentifier>(1000, Allocator.Persistent);
            _createdComponentsIdentifier = new NativeHashSet<ComponentIdentifier>(1000, Allocator.Persistent);
            _destroyedComponentsIdentifier = new NativeHashSet<ComponentIdentifier>(1000, Allocator.Persistent);
            OnCreate(recorderContext);
        }

        void IRecorderModule.Destroy(RecorderContext recorderContext)
        {
            OnDestroy(recorderContext);
            _recordedComponentsIdentifier.Dispose();
            _createdComponentsIdentifier.Dispose();
            _destroyedComponentsIdentifier.Dispose();
        }

        public void StartRecordingObject(ComponentSafeRef<TC> objSafeRef, bool markCreated, Record record,
            RecorderContext ctx)
        {
            CheckIsRecording();

            if (!_recordedComponentsIdentifier.Add(objSafeRef.Identifier))
                throw new InvalidOperationException("Object is already being recorded.");

            _recordedComponents.Add(objSafeRef);
            OnStartRecordingObject(objSafeRef, record, ctx);

            if (!markCreated)
                return;

            var markedCreated = _createdComponentsIdentifier.Add(objSafeRef.Identifier);
            _destroyedComponentsIdentifier.Remove(objSafeRef.Identifier);

            if (markedCreated)
                OnObjectMarkedCreated(objSafeRef);
        }

        public void StopRecordingObject(ComponentSafeRef<TC> objSafeRef, bool markDestroyed, Record record,
            RecorderContext ctx)
        {
            CheckIsRecording();

            if (!_recordedComponentsIdentifier.Contains(objSafeRef.Identifier))
                throw new InvalidOperationException("Object is not being recorded.");

            // Deferred removal (after frame data collection)
            _componentsToRemove.Add(objSafeRef);

            if (markDestroyed)
            {
                var markedDestroyed = _destroyedComponentsIdentifier.Add(objSafeRef.Identifier);
                _createdComponentsIdentifier.Remove(objSafeRef.Identifier);

                if (markedDestroyed)
                    OnObjectMarkedDestroyed(objSafeRef);
            }
        }

        public void StartRecordingObject(IObjectSafeRef objSafeRef, bool markCreated, Record record,
            RecorderContext ctx)
        {
            if (objSafeRef is not ComponentSafeRef<TC> componentSafeRef)
            {
                throw new InvalidOperationException($"Object is not of type {typeof(TC).Name}");
            }

            StartRecordingObject(componentSafeRef, markCreated, record, ctx);
        }

        public void StopRecordingObject(IObjectSafeRef objSafeRef, bool markDestroyed, Record record,
            RecorderContext ctx)
        {
            if (objSafeRef is not ComponentSafeRef<TC> componentSafeRef)
            {
                throw new InvalidOperationException($"Object is not of type {typeof(TC).Name}");
            }

            StopRecordingObject(componentSafeRef, markDestroyed, record, ctx);
        }

        public bool IsObjectSupported(IObjectSafeRef objSafeRef)
        {
            return objSafeRef is ComponentSafeRef<TC>;
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
            _createdComponentsIdentifier.Clear();
            _destroyedComponentsIdentifier.Clear();

            foreach (var toRemove in _componentsToRemove)
            {
                _recordedComponentsIdentifier.Remove(toRemove.Identifier);
                _recordedComponents.Remove(toRemove);
                OnStopRecordingObject(toRemove, record, context);
            }

            _componentsToRemove.Clear();
        }

        bool IFrameDataRecorderModule.SerializeFrameData(FrameInfo frameInfo, FrameDataWriter frameDataWriter)
        {
            TD frameData;

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
            return objSafeRef is ComponentSafeRef<TC> componentSafeRef && _recordedComponents.Contains(componentSafeRef);
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

            _recordedComponents.Clear();
            _createdComponentsIdentifier.Clear();
            _destroyedComponentsIdentifier.Clear();

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

        protected virtual void OnStartRecordingObject(ComponentSafeRef<TC> objSafeRef, Record record,
            RecorderContext ctx)
        {
        }

        protected virtual void OnStopRecordingObject(ComponentSafeRef<TC> objSafeRef, Record record,
            RecorderContext recorderContext)
        {
        }

        protected virtual void OnObjectMarkedCreated(ComponentSafeRef<TC> objSafeRef)
        {
        }

        protected virtual void OnObjectMarkedDestroyed(ComponentSafeRef<TC> objSafeRef)
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

        protected abstract TD CollectFrameData(FrameInfo frameInfo, Record record, RecorderContext context);

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