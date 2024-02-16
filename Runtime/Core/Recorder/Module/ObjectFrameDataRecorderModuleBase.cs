using System.Collections.Generic;

namespace PLUME.Core.Recorder.Module
{
    public abstract class ObjectFrameDataRecorderModuleBase<TObject, TFrameData> : ObjectRecorderModuleBase<TObject>,
        IFrameDataRecorderModule where TObject : UnityEngine.Object where TFrameData : unmanaged, IFrameData
    {
        private readonly Dictionary<Frame, TFrameData> _framesData = new(FrameComparer.Instance);

        void IFrameDataRecorderModule.PushFrameData(Frame frame)
        {
            var frameData = CollectFrameData();
            ClearCreatedObjects();
            ClearDestroyedObjects();

            lock (_framesData)
            {
                _framesData.Add(frame, frameData);
            }
        }

        bool IFrameDataRecorderModule.TryPopSerializedFrameData(Frame frame, SerializedSamplesBuffer buffer)
        {
            TFrameData frameData;
            
            lock (_framesData)
            {
                if (!_framesData.TryGetValue(frame, out frameData))
                {
                    return false;
                }

                _framesData.Remove(frame);
            }

            SerializeFrameData(frameData, buffer);
            DisposeFrameData(frameData);
            return true;
        }

        protected abstract TFrameData CollectFrameData();

        protected abstract void SerializeFrameData(TFrameData frameData, SerializedSamplesBuffer buffer);

        protected abstract void DisposeFrameData(TFrameData frameData);
    }
}