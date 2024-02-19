using System.Collections.Generic;
using PLUME.Core.Recorder.Data;

namespace PLUME.Core.Recorder.Module
{
    public abstract class ObjectFrameDataRecorderModuleBase<TObject, TFrameData> : ObjectRecorderModuleBase<TObject>,
        IFrameDataRecorderModule where TObject : UnityEngine.Object where TFrameData : unmanaged, IFrameData
    {
        private readonly Dictionary<Frame, TFrameData> _framesData = new(FrameComparer.Instance);

        void IFrameDataRecorderModule.CollectFrameData(Frame frame)
        {
            var frameData = OnCollectFrameData(frame);
            ClearCreatedObjects();
            ClearDestroyedObjects();

            lock (_framesData)
            {
                _framesData.Add(frame, frameData);
            }
        }

        bool IFrameDataRecorderModule.SerializeFrameData(Frame frame, FrameDataWriter output)
        {
            TFrameData frameData;
            
            lock (_framesData)
            {
                if (!_framesData.TryGetValue(frame, out frameData))
                {
                    return false;
                }
            }

            OnSerializeFrameData(frameData, frame, output);
            return true;
        }
        
        void IFrameDataRecorderModule.DisposeFrameData(Frame frame)
        {
            TFrameData frameData;
            
            lock (_framesData)
            {
                if (!_framesData.TryGetValue(frame, out frameData))
                {
                    return;
                }
                _framesData.Remove(frame);
            }

            OnDisposeFrameData(frameData, frame);
        }

        protected abstract TFrameData OnCollectFrameData(Frame frame);

        protected abstract void OnSerializeFrameData(TFrameData frameData, Frame frame, FrameDataWriter output);

        protected abstract void OnDisposeFrameData(TFrameData frameData, Frame frame);
    }
}