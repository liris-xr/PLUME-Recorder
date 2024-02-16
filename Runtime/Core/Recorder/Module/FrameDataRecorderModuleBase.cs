using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace PLUME.Core.Recorder.Module
{
    public abstract class FrameDataRecorderModuleBase<TFrameData> : IFrameDataRecorderModule
        where TFrameData : unmanaged, IFrameData
    {
        private readonly Dictionary<Frame, TFrameData> _framesData = new(FrameComparer.Instance);

        public bool IsRecording { get; private set; }

        private TFrameData _frameData;

        void IRecorderModule.Start(RecordContext recordContext, RecorderContext recorderContext)
        {
            if (IsRecording)
                throw new InvalidOperationException("Recorder module is already recording.");
            OnStart(recordContext, recorderContext);
            IsRecording = true;
        }

        void IRecorderModule.ForceStop(RecordContext recordContext, RecorderContext recorderContext)
        {
            EnsureIsRecording();
            OnForceStop(recordContext, recorderContext);
            IsRecording = false;
        }

        async UniTask IRecorderModule.Stop(RecordContext recordContext, RecorderContext recorderContext)
        {
            EnsureIsRecording();
            await OnStop(recordContext, recorderContext);
            IsRecording = false;
        }

        void IRecorderModule.Reset(RecorderContext recorderContext)
        {
            OnReset(recorderContext);
        }

        void IFrameDataRecorderModule.PushFrameData(Frame frame)
        {
            var frameData = CollectFrameData();

            lock (_framesData)
            {
                _framesData.Add(frame, frameData);
            }
        }

        bool IFrameDataRecorderModule.TryPopSerializedFrameData(Frame frame, SerializedSamplesBuffer buffer)
        {
            TFrameData frameData;
            
            lock (_framesData)
            {
                if (!_framesData.TryGetValue(frame, out frameData))
                {
                    return false;
                }

                _framesData.Remove(frame);
            }

            SerializeFrameData(frameData, buffer);
            DisposeFrameData(frameData);
            return true;
        }

        protected void EnsureIsRecording()
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

        void IRecorderModule.FixedUpdate(RecordContext recordContext, RecorderContext context)
        {
            OnFixedUpdate(recordContext, context);
        }

        void IRecorderModule.EarlyUpdate(RecordContext recordContext, RecorderContext context)
        {
            OnEarlyUpdate(recordContext, context);
        }

        void IRecorderModule.PreUpdate(RecordContext recordContext, RecorderContext context)
        {
            OnPreUpdate(recordContext, context);
        }

        void IRecorderModule.Update(RecordContext recordContext, RecorderContext context)
        {
        }

        void IRecorderModule.PreLateUpdate(RecordContext recordContext, RecorderContext context)
        {
            OnPreLateUpdate(recordContext, context);
        }

        void IRecorderModule.PostLateUpdate(RecordContext recordContext, RecorderContext context)
        {
            OnPostLateUpdate(recordContext, context);
        }

        protected virtual void OnFixedUpdate(RecordContext recordContext, RecorderContext context)
        {
        }

        protected virtual void OnEarlyUpdate(RecordContext recordContext, RecorderContext context)
        {
        }

        protected virtual void OnPreUpdate(RecordContext recordContext, RecorderContext context)
        {
        }

        protected virtual void OnUpdate(RecordContext recordContext, RecorderContext context)
        {
        }

        protected virtual void OnPreLateUpdate(RecordContext recordContext, RecorderContext context)
        {
        }

        protected virtual void OnPostLateUpdate(RecordContext recordContext, RecorderContext context)
        {
        }

        protected virtual void OnCreate(RecorderContext recorderContext)
        {
        }

        protected virtual void OnDestroy(RecorderContext recorderContext)
        {
        }

        protected virtual void OnStart(RecordContext recordContext, RecorderContext recorderContext)
        {
        }

        protected void OnForceStop(RecordContext recordContext, RecorderContext recorderContext)
        {
        }

        protected virtual UniTask OnStop(RecordContext recordContext, RecorderContext recorderContext)
        {
            return UniTask.CompletedTask;
        }

        protected virtual void OnReset(RecorderContext recorderContext)
        {
        }

        protected abstract TFrameData CollectFrameData();

        protected abstract void SerializeFrameData(TFrameData frameData, SerializedSamplesBuffer buffer);

        protected abstract void DisposeFrameData(TFrameData frameData);
    }
}