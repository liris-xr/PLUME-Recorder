using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace PLUME.Core.Recorder.Module
{
    public abstract class FrameDataRecorderModuleBase : IFrameDataRecorderModule
    {
        public bool IsRecording { get; private set; }
        
        void IRecorderModule.Create(RecorderContext recorderContext)
        {
            OnCreate(recorderContext);
        }

        void IRecorderModule.Destroy(RecorderContext recorderContext)
        {
            OnDestroy(recorderContext);
        }

        void IRecorderModule.Start(RecordContext recordContext, RecorderContext recorderContext)
        {
            if (IsRecording)
                throw new InvalidOperationException("Recorder module is already recording.");
            OnStart(recordContext, recorderContext);
            IsRecording = true;
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

        async UniTask IFrameDataRecorderModule.RecordFrameData(SerializedSamplesBuffer buffer, CancellationToken cancellationToken)
        {
            await OnRecordFrameData(buffer, cancellationToken);
        }
        
        protected void EnsureIsRecording()
        {
            if (!IsRecording)
            {
                throw new InvalidOperationException("Recorder module is not recording.");
            }
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

        protected virtual UniTask OnStop(RecordContext recordContext, RecorderContext recorderContext)
        {
            return UniTask.CompletedTask;
        }

        protected virtual void OnReset(RecorderContext recorderContext)
        {
        }

        protected virtual UniTask OnRecordFrameData(SerializedSamplesBuffer buffer, CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }
    }
}