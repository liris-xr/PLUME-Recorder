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

        void IRecorderModule.Start(RecordContext recordContext, RecorderContext recorderContext)
        {
            if (IsRecording)
                throw new InvalidOperationException("Recorder module is already recording.");

            IsRecording = true;
            OnStart(recordContext, recorderContext);
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

        void IRecorderModule.Reset(RecorderContext ctx)
        {
            OnReset(ctx);
        }

        protected void EnsureIsRecording()
        {
            if (!IsRecording)
            {
                throw new InvalidOperationException("Recorder module is not recording.");
            }
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

        protected virtual void OnCreate(RecorderContext ctx)
        {
        }

        protected virtual void OnDestroy(RecorderContext ctx)
        {
        }

        protected virtual void OnStart(RecordContext recordContext, RecorderContext recorderContext)
        {
        }

        protected virtual void OnForceStop(RecordContext recordContext, RecorderContext recorderContext)
        {
        }

        protected virtual UniTask OnStop(RecordContext recordContext, RecorderContext recorderContext)
        {
            return UniTask.CompletedTask;
        }

        protected virtual void OnReset(RecorderContext ctx)
        {
        }
    }
}