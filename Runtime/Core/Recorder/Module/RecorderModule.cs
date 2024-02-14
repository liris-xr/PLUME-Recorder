using System;
using System.Threading;
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

        protected virtual void OnCreate(RecorderContext ctx)
        {
        }

        protected virtual void OnDestroy(RecorderContext ctx)
        {
        }

        protected virtual void OnStart(RecordContext recordContext, RecorderContext recorderContext)
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