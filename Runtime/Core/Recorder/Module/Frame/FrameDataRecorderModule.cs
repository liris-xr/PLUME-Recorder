using System.Collections.Generic;

namespace PLUME.Core.Recorder.Module.Frame
{
    public abstract class FrameDataRecorderModule<TFrameData> : IFrameDataRecorderModule where TFrameData : IFrameData
    {
        private readonly Dictionary<FrameInfo, TFrameData> _framesData = new(FrameInfoComparer.Instance);
        
        void IFrameDataRecorderModule.BeforeEnqueueFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            BeforeCollectFrameData(frameInfo, ctx);
        }
        
        void IFrameDataRecorderModule.EnqueueFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            var frameData = CollectFrameData(frameInfo, ctx);
            
            if(frameData == null)
                return;

            lock (_framesData)
            {
                _framesData.Add(frameInfo, frameData);
            }
        }
        
        void IFrameDataRecorderModule.AfterEnqueueFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            AfterCollectFrameData(frameInfo, ctx);
        }
        
        void IFrameDataRecorderModule.SerializeFrameData(FrameInfo frameInfo, FrameDataWriter frameDataWriter)
        {
            TFrameData frameData;

            lock (_framesData)
            {
                if (!_framesData.TryGetValue(frameInfo, out frameData))
                {
                    return;
                }
            }

            frameData.Serialize(frameDataWriter);
            frameData.Dispose();
        }
        
        protected abstract TFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx);
        
        protected virtual void BeforeCollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
        }
        
        protected virtual void AfterCollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
        }

        void IRecorderModule.Create(RecorderContext ctx)
        {
            OnCreate(ctx);
        }

        void IRecorderModule.Destroy(RecorderContext ctx)
        {
            OnDestroy(ctx);
        }

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

        void IFrameDataRecorderModule.FixedUpdate(ulong fixedDeltaTime, RecorderContext context)
        {
            OnFixedUpdate(fixedDeltaTime, context);
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

        protected virtual void OnAwake(RecorderContext ctx)
        {
        }
        
        protected virtual void OnCreate(RecorderContext ctx)
        {
        }
        
        protected virtual void OnDestroy(RecorderContext ctx)
        {
        }
        
        protected virtual void OnStartRecording(RecorderContext ctx)
        {
        }
        
        protected virtual void OnStopRecording(RecorderContext ctx)
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
    }
}