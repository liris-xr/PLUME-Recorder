using System;
using System.Collections.Generic;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module;
using PLUME.Core.Recorder.Module.Frame;

namespace PLUME.Base.Module
{
    public abstract class FrameDataRecorderModuleBase<TFrameData> : IFrameDataRecorderModule
        where TFrameData : IFrameData
    {
        private readonly Dictionary<FrameInfo, TFrameData> _framesData = new(FrameInfoComparer.Instance);

        public bool IsRecording { get; private set; }

        private TFrameData _frameData;

        void IRecorderModule.Awake(RecorderContext context)
        {
            OnAwake(context);
        }

        void IRecorderModule.StartRecording(Record record, RecorderContext recorderContext)
        {
            if (IsRecording)
                throw new InvalidOperationException("Recorder module is already recording.");
            OnStartRecording(record, recorderContext);
            IsRecording = true;
        }

        void IRecorderModule.StopRecording(Record record, RecorderContext recorderContext)
        {
            CheckIsRecording();
            OnStopRecording(record, recorderContext);
            IsRecording = false;
        }

        // ReSharper restore Unity.PerformanceCriticalContext

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
            
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        bool IFrameDataRecorderModule.SerializeFrameData(FrameInfo frameInfo, FrameDataWriter frameDataWriter)
        {
            TFrameData frameData;

            lock (_framesData)
            {
                if (!_framesData.Remove(frameInfo, out frameData))
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

        protected void CheckIsRecording()
        {
            if (!IsRecording)
            {
                throw new InvalidOperationException("Recorder module is not recording.");
            }
        }

        void IRecorderModule.Create(RecorderContext recorderContext)
        {
            OnCreate(recorderContext);
        }

        void IRecorderModule.Destroy(RecorderContext recorderContext)
        {
            OnDestroy(recorderContext);
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

        protected virtual void OnAwake(RecorderContext context)
        {
        }

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

        // ReSharper restore Unity.PerformanceCriticalContext

        protected virtual void OnCreate(RecorderContext recorderContext)
        {
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        protected virtual void OnDestroy(RecorderContext recorderContext)
        {
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        protected virtual void OnStartRecording(Record record, RecorderContext recorderContext)
        {
        }

        protected virtual void OnStopRecording(Record record, RecorderContext recorderContext)
        {
        }

        // ReSharper restore Unity.PerformanceCriticalContext
        protected abstract TFrameData CollectFrameData(FrameInfo frameInfo, Record record, RecorderContext context);
    }
}