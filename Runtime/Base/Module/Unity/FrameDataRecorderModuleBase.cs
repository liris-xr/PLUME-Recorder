using System;
using System.Collections.Generic;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module;
using PLUME.Core.Recorder.Module.Frame;

namespace PLUME.Base.Module.Unity
{
    public abstract class FrameDataRecorderModuleBase<TFrameData> : IFrameDataRecorderModule
        where TFrameData : IFrameData
    {
        private readonly Dictionary<FrameInfo, TFrameData> _framesData = new(FrameInfoComparer.Instance);

        private TFrameData _frameData;

        void IRecorderModule.Awake(RecorderContext ctx)
        {
            OnAwake(ctx);
        }

        void IRecorderModule.StartRecording(RecorderContext ctx)
        {
            OnStartRecording(ctx);
        }

        void IRecorderModule.StopRecording(RecorderContext ctx)
        {
            OnStopRecording(ctx);
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        void IFrameDataRecorderModule.EnqueueFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            var frameData = CollectFrameData(frameInfo, ctx);

            lock (_framesData)
            {
                _framesData.Add(frameInfo, frameData);
            }
        }

        void IFrameDataRecorderModule.PostEnqueueFrameData(RecorderContext ctx)
        {
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        void IFrameDataRecorderModule.SerializeFrameData(FrameInfo frameInfo, FrameDataWriter frameDataWriter)
        {
            TFrameData frameData;

            lock (_framesData)
            {
                if (!_framesData.Remove(frameInfo, out frameData))
                {
                    return;
                }
            }

            frameData.Serialize(frameDataWriter);

            if (frameData is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        protected void CheckIsRecording(RecorderContext ctx)
        {
            if (!ctx.IsRecording)
            {
                throw new InvalidOperationException("Recorder module is not recording.");
            }
        }

        void IRecorderModule.Create(RecorderContext ctx)
        {
            OnCreate(ctx);
        }

        void IRecorderModule.Destroy(RecorderContext ctx)
        {
            OnDestroy(ctx);
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        void IFrameDataRecorderModule.FixedUpdate(ulong fixedDeltaTime, RecorderContext ctx)
        {
            OnFixedUpdate(fixedDeltaTime, ctx);
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        void IFrameDataRecorderModule.EarlyUpdate(ulong deltaTime, RecorderContext ctx)
        {
            OnEarlyUpdate(deltaTime, ctx);
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        void IFrameDataRecorderModule.PreUpdate(ulong deltaTime, RecorderContext ctx)
        {
            OnPreUpdate(deltaTime, ctx);
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        void IFrameDataRecorderModule.Update(ulong deltaTime, RecorderContext ctx)
        {
            OnUpdate(deltaTime, ctx);
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        void IFrameDataRecorderModule.PreLateUpdate(ulong deltaTime, RecorderContext ctx)
        {
            OnPreLateUpdate(deltaTime, ctx);
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        void IFrameDataRecorderModule.PostLateUpdate(ulong deltaTime, RecorderContext ctx)
        {
            OnPostLateUpdate(deltaTime, ctx);
        }

        protected virtual void OnAwake(RecorderContext context)
        {
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        protected virtual void OnFixedUpdate(ulong fixedDeltaTime, RecorderContext ctx)
        {
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        protected virtual void OnEarlyUpdate(ulong deltaTime, RecorderContext ctx)
        {
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        protected virtual void OnPreUpdate(ulong deltaTime, RecorderContext ctx)
        {
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        protected virtual void OnUpdate(ulong deltaTime, RecorderContext ctx)
        {
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        protected virtual void OnPreLateUpdate(ulong deltaTime, RecorderContext ctx)
        {
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        protected virtual void OnPostLateUpdate(ulong deltaTime, RecorderContext ctx)
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

        protected virtual void OnStartRecording(RecorderContext ctx)
        {
        }

        protected virtual void OnStopRecording(RecorderContext ctx)
        {
        }

        // ReSharper restore Unity.PerformanceCriticalContext
        protected abstract TFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx);
    }
}