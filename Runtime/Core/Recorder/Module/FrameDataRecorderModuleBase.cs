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

        void IFrameDataRecorderModule.CollectFrameData(Frame frame)
        {
            var frameData = OnCollectFrameData(frame);

            lock (_framesData)
            {
                _framesData.Add(frame, frameData);
            }
        }

        bool IFrameDataRecorderModule.SerializeFrameData(Frame frame, FrameDataChunks output)
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

            OnSerializeFrameData(frameData, frame, output);
            return true;
        }

        void IFrameDataRecorderModule.DisposeFrameData(Frame frame)
        {
            TFrameData frameData;
            
            lock (_framesData)
            {
                if (!_framesData.TryGetValue(frame, out frameData))
                {
                    return;
                }
                _framesData.Remove(frame);
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

        void IRecorderModule.FixedUpdate(Record record, RecorderContext context)
        {
            OnFixedUpdate(record, context);
        }

        void IRecorderModule.EarlyUpdate(Record record, RecorderContext context)
        {
            OnEarlyUpdate(record, context);
        }

        void IRecorderModule.PreUpdate(Record record, RecorderContext context)
        {
            OnPreUpdate(record, context);
        }

        void IRecorderModule.Update(Record record, RecorderContext context)
        {
        }

        void IRecorderModule.PreLateUpdate(Record record, RecorderContext context)
        {
            OnPreLateUpdate(record, context);
        }

        void IRecorderModule.PostLateUpdate(Record record, RecorderContext context)
        {
            OnPostLateUpdate(record, context);
        }

        protected virtual void OnFixedUpdate(Record record, RecorderContext context)
        {
        }

        protected virtual void OnEarlyUpdate(Record record, RecorderContext context)
        {
        }

        protected virtual void OnPreUpdate(Record record, RecorderContext context)
        {
        }

        protected virtual void OnUpdate(Record record, RecorderContext context)
        {
        }

        protected virtual void OnPreLateUpdate(Record record, RecorderContext context)
        {
        }

        protected virtual void OnPostLateUpdate(Record record, RecorderContext context)
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

        protected abstract TFrameData OnCollectFrameData(Frame frame);

        protected abstract void OnSerializeFrameData(TFrameData frameData, Frame frame, FrameDataChunks output);

        protected abstract void OnDisposeFrameData(TFrameData frameData, Frame frame);
    }
}