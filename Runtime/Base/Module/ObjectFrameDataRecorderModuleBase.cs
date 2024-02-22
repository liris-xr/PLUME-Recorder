using System;
using System.Collections.Generic;
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

        protected abstract TFrameData CollectFrameData(FrameInfo frameInfo);
    }
}