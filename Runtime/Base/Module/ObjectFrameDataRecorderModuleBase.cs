using System;
using System.Collections.Generic;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using Object = UnityEngine.Object;

namespace PLUME.Base.Module
{
    public abstract class ObjectFrameDataRecorderModuleBase<TObject, TFrameData> : ObjectRecorderModuleBase<TObject>,
        IFrameDataRecorderModule where TObject : Object where TFrameData : IFrameData
    {
        private readonly Dictionary<FrameInfo, TFrameData> _framesData = new(FrameInfoComparer.Instance);

        void IFrameDataRecorderModule.EnqueueFrameData(FrameInfo frameInfo)
        {
            var frameData = CollectFrameData(frameInfo);
            ClearCreatedObjects();
            ClearDestroyedObjects();

            lock (_framesData)
            {
                _framesData.Add(frameInfo, frameData);
            }
        }

        bool IFrameDataRecorderModule.SerializeFrameData(FrameInfo frameInfo, FrameDataWriter frameDataWriter)
        {
            TFrameData frameData;

            lock (_framesData)
            {
                if (!_framesData.TryGetValue(frameInfo, out frameData))
                {
                    return false;
                }
            }

            frameData.Serialize(frameDataWriter);

            if (frameData is IDisposable disposable)
            {
                disposable.Dispose();
            }
            
            return true;
        }
        
        // ReSharper restore Unity.PerformanceCriticalContext

        void IFrameDataRecorderModule.FixedUpdate(long fixedDeltaTime, Record record, RecorderContext context)
        {
            OnFixedUpdate(fixedDeltaTime, record, context);
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        void IFrameDataRecorderModule.EarlyUpdate(long deltaTime, Record record, RecorderContext context)
        {
            OnEarlyUpdate(deltaTime, record, context);
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        void IFrameDataRecorderModule.PreUpdate(long deltaTime, Record record, RecorderContext context)
        {
            OnPreUpdate(deltaTime, record, context);
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        void IFrameDataRecorderModule.Update(long deltaTime, Record record, RecorderContext context)
        {
            OnUpdate(deltaTime, record, context);
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        void IFrameDataRecorderModule.PreLateUpdate(long deltaTime, Record record, RecorderContext context)
        {
            OnPreLateUpdate(deltaTime, record, context);
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        void IFrameDataRecorderModule.PostLateUpdate(long deltaTime, Record record, RecorderContext context)
        {
            OnPostLateUpdate(deltaTime, record, context);
        }

        protected abstract TFrameData CollectFrameData(FrameInfo frameInfo);
        
        
        // ReSharper restore Unity.PerformanceCriticalContext

        protected virtual void OnFixedUpdate(long fixedDeltaTime, Record record, RecorderContext context)
        {
        }
        
        // ReSharper restore Unity.PerformanceCriticalContext

        protected virtual void OnEarlyUpdate(long deltaTime, Record record, RecorderContext context)
        {
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        protected virtual void OnPreUpdate(long deltaTime, Record record, RecorderContext context)
        {
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        protected virtual void OnUpdate(long deltaTime, Record record, RecorderContext context)
        {
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        protected virtual void OnPreLateUpdate(long deltaTime, Record record, RecorderContext context)
        {
        }

        // ReSharper restore Unity.PerformanceCriticalContext

        protected virtual void OnPostLateUpdate(long deltaTime, Record record, RecorderContext context)
        {
        }
    }
}