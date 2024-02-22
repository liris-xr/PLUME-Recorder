using System;
using Cysharp.Threading.Tasks;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module;

namespace PLUME.Base.Module
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
            OnForceStopRecording(record, recorderContext);
            IsRecording = false;
        }

        protected void CheckIsRecording()
        {
            if (!IsRecording)
            {
                throw new InvalidOperationException("Recorder module is not recording.");
            }
        }

        protected virtual void OnAwake(RecorderContext context)
        {
        }

        protected virtual void OnEarlyUpdate(long deltaTime, Record record, RecorderContext context)
        {
        }

        protected virtual void OnPreUpdate(long deltaTime, Record record, RecorderContext context)
        {
        }

        protected virtual void OnUpdate(long deltaTime, Record record, RecorderContext context)
        {
        }

        protected virtual void OnPreLateUpdate(long deltaTime, Record record, RecorderContext context)
        {
        }

        protected virtual void OnPostLateUpdate(long deltaTime, Record record, RecorderContext context)
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
    }
}