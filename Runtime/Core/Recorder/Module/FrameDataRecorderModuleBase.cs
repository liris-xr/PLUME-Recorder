using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PLUME.Core.Recorder.Data;

namespace PLUME.Core.Recorder.Module
{
    public abstract class FrameDataRecorderModuleBase<TFrameData> : IFrameDataRecorderModule
        where TFrameData : unmanaged, IFrameData
    {
        private readonly Dictionary<Frame, TFrameData> _framesData = new(FrameComparer.Instance);

        public bool IsRecording { get; private set; }

        private TFrameData _frameData;

        void IRecorderModule.StartRecording(Record record, RecorderContext recorderContext)
        {
            if (IsRecording)
                throw new InvalidOperationException("Recorder module is already recording.");
            OnStartRecording(record, recorderContext);
            IsRecording = true;
        }

        void IRecorderModule.ForceStopRecording(Record record, RecorderContext recorderContext)
        {
            CheckIsRecording();
            OnForceStopRecording(record, recorderContext);
            IsRecording = false;
        }

        async UniTask IRecorderModule.StopRecording(Record record, RecorderContext recorderContext)
        {
            CheckIsRecording();
            await OnStopRecording(record, recorderContext);
            IsRecording = false;
        }

        void IRecorderModule.Reset(RecorderContext recorderContext)
        {
            OnReset(recorderContext);
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        void IFrameDataRecorderModule.CollectFrameData(Frame frame)
        {
            var frameData = OnCollectFrameData(frame);

            lock (_framesData)
            {
                _framesData.Add(frame, frameData);
            }
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        bool IFrameDataRecorderModule.SerializeFrameData(Frame frame, FrameDataWriter output)
        {
            TFrameData frameData;

            lock (_framesData)
            {
                if (!_framesData.Remove(frame, out frameData))
                {
                    return false;
                }
            }

            OnSerializeFrameData(frameData, frame, output);
            return true;
        }

        // ReSharper restore Unity.PerformanceCriticalContext
        void IFrameDataRecorderModule.DisposeFrameData(Frame frame)
        {
            TFrameData frameData;

            lock (_framesData)
            {
                if (!_framesData.Remove(frame, out frameData))
                {
                    return;
                }
            }

            OnDisposeFrameData(frameData, frame);
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
        void IRecorderModule.FixedUpdate(Record record, RecorderContext context)
        {
            OnFixedUpdate(record, context);
        }

        // ReSharper restore Unity.PerformanceCriticalContext
        void IRecorderModule.EarlyUpdate(Record record, RecorderContext context)
        {
            OnEarlyUpdate(record, context);
        }

        // ReSharper restore Unity.PerformanceCriticalContext
        void IRecorderModule.PreUpdate(Record record, RecorderContext context)
        {
            OnPreUpdate(record, context);
        }

        // ReSharper restore Unity.PerformanceCriticalContext
        void IRecorderModule.Update(Record record, RecorderContext context)
        {
        }

        // ReSharper restore Unity.PerformanceCriticalContext
        void IRecorderModule.PreLateUpdate(Record record, RecorderContext context)
        {
            OnPreLateUpdate(record, context);
        }

        // ReSharper restore Unity.PerformanceCriticalContext
        void IRecorderModule.PostLateUpdate(Record record, RecorderContext context)
        {
            OnPostLateUpdate(record, context);
        }

        // ReSharper restore Unity.PerformanceCriticalContext
        protected virtual void OnFixedUpdate(Record record, RecorderContext context)
        {
        }

        // ReSharper restore Unity.PerformanceCriticalContext
        protected virtual void OnEarlyUpdate(Record record, RecorderContext context)
        {
        }

        // ReSharper restore Unity.PerformanceCriticalContext
        protected virtual void OnPreUpdate(Record record, RecorderContext context)
        {
        }

        // ReSharper restore Unity.PerformanceCriticalContext
        protected virtual void OnUpdate(Record record, RecorderContext context)
        {
        }

        // ReSharper restore Unity.PerformanceCriticalContext
        protected virtual void OnPreLateUpdate(Record record, RecorderContext context)
        {
        }

        // ReSharper restore Unity.PerformanceCriticalContext
        protected virtual void OnPostLateUpdate(Record record, RecorderContext context)
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
        
        protected virtual UniTask OnStopRecording(Record record, RecorderContext recorderContext)
        {
            return UniTask.CompletedTask;
        }

        protected void OnForceStopRecording(Record record, RecorderContext recorderContext)
        {
        }

        protected virtual void OnReset(RecorderContext recorderContext)
        {
        }

        // ReSharper restore Unity.PerformanceCriticalContext
        protected abstract TFrameData OnCollectFrameData(Frame frame);

        // ReSharper restore Unity.PerformanceCriticalContext
        protected abstract void OnSerializeFrameData(TFrameData frameData, Frame frame, FrameDataWriter output);

        // ReSharper restore Unity.PerformanceCriticalContext
        protected abstract void OnDisposeFrameData(TFrameData frameData, Frame frame);
    }
}