using System.Collections.Generic;
using PLUME.Core.Recorder.Module;
using PLUME.Core.Recorder.Module.Frame;
using UnityEngine;

namespace PLUME.Base.Module
{
    public abstract class ObjectFrameDataRecorderModuleBase<TObject, TFrameData> : ObjectRecorderModuleBase<TObject>,
        IFrameDataRecorderModule where TObject : Object
    {
        private readonly Dictionary<FrameInfo, TFrameData> _framesData = new(FrameInfoComparer.Instance);

        void IFrameDataRecorderModule.CollectFrameData(FrameInfo frameInfo)
        {
            var frameData = OnCollectFrameData(frameInfo);
            ClearCreatedObjects();
            ClearDestroyedObjects();

            lock (_framesData)
            {
                _framesData.Add(frameInfo, frameData);
            }
        }

        bool IFrameDataRecorderModule.SerializeFrameData(FrameInfo frameInfo, FrameDataWriter output)
        {
            TFrameData frameData;

            lock (_framesData)
            {
                if (!_framesData.TryGetValue(frameInfo, out frameData))
                {
                    return false;
                }
            }

            OnSerializeFrameData(frameData, frameInfo, output);
            return true;
        }

        void IFrameDataRecorderModule.DisposeFrameData(FrameInfo frameInfo)
        {
            TFrameData frameData;

            lock (_framesData)
            {
                if (!_framesData.TryGetValue(frameInfo, out frameData))
                {
                    return;
                }

                _framesData.Remove(frameInfo);
            }

            OnDisposeFrameData(frameData, frameInfo);
        }

        protected abstract TFrameData OnCollectFrameData(FrameInfo frameInfo);

        protected abstract void OnSerializeFrameData(TFrameData frameData, FrameInfo frameInfo, FrameDataWriter output);

        protected abstract void OnDisposeFrameData(TFrameData frameData, FrameInfo frameInfo);
    }
}