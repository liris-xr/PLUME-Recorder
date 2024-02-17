using System;
using Cysharp.Threading.Tasks;

namespace PLUME.Core.Recorder.Module
{
    public abstract class RecorderModuleBase : IRecorderModule
    {
        public bool IsRecording { get; private set; }

        void IRecorderModule.Create(RecorderContext ctx)
        {
            OnCreate(ctx);
        }

        void IRecorderModule.Destroy(RecorderContext ctx)
        {
            OnDestroy(ctx);
        }

        void IRecorderModule.StartRecording(Record record, RecorderContext recorderContext)
        {
            if (IsRecording)
                throw new InvalidOperationException("Recorder module is already recording.");

            IsRecording = true;
            OnStartRecording(record, recorderContext);
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

        void IRecorderModule.Reset(RecorderContext ctx)
        {
            OnReset(ctx);
        }

        protected void CheckIsRecording()
        {
            if (!IsRecording)
            {
                throw new InvalidOperationException("Recorder module is not recording.");
            }
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

        protected virtual void OnCreate(RecorderContext ctx)
        {
        }

        protected virtual void OnDestroy(RecorderContext ctx)
        {
        }

        protected virtual void OnStartRecording(Record record, RecorderContext recorderContext)
        {
        }
        
        protected virtual UniTask OnStopRecording(Record record, RecorderContext recorderContext)
        {
            return UniTask.CompletedTask;
        }
        
        protected virtual void OnForceStopRecording(Record record, RecorderContext recorderContext)
        {
        }

        protected virtual void OnReset(RecorderContext ctx)
        {
        }
    }
}