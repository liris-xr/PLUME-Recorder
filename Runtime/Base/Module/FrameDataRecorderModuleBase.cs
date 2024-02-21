using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module;
using PLUME.Core.Recorder.Module.Frame;

namespace PLUME.Base.Module
{
    public abstract class FrameDataRecorderModuleBase<TFrameData> : IFrameDataRecorderModule
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

        // ReSharper restore Unity.PerformanceCriticalContext

        void IFrameDataRecorderModule.CollectFrameData(Core.Recorder.Module.Frame.FrameInfo frameInfo)
        {
            var frameData = OnCollectFrameData(frameInfo);

            lock (_framesData)
            {
                _framesData.Add(frameInfo, frameData);
            }
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        bool IFrameDataRecorderModule.SerializeFrameData(Core.Recorder.Module.Frame.FrameInfo frameInfo,
            FrameDataWriter output)
        {
            TFrameData frameData;

            lock (_framesData)
            {
                if (!_framesData.Remove(frameInfo, out frameData))
                {
                    return false;
                }
            }

            OnSerializeFrameData(frameData, frameInfo, output);
            return true;
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        void IFrameDataRecorderModule.DisposeFrameData(Core.Recorder.Module.Frame.FrameInfo frameInfo)
        {
            TFrameData frameData;

            lock (_framesData)
            {
                if (!_framesData.Remove(frameInfo, out frameData))
                {
                    return;
                }
            }

            OnDisposeFrameData(frameData, frameInfo);
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

        protected virtual void OnAwake(RecorderContext context)
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

        // ReSharper restore Unity.PerformanceCriticalContext
        protected abstract TFrameData OnCollectFrameData(Core.Recorder.Module.Frame.FrameInfo frameInfo);

        // ReSharper restore Unity.PerformanceCriticalContext

        protected abstract void OnSerializeFrameData(TFrameData frameData,
            Core.Recorder.Module.Frame.FrameInfo frameInfo, FrameDataWriter output);

        // ReSharper restore Unity.PerformanceCriticalContext

        protected abstract void OnDisposeFrameData(TFrameData frameData,
            Core.Recorder.Module.Frame.FrameInfo frameInfo);
    }
}