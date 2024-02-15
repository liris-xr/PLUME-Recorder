using System.Collections.Concurrent;

namespace PLUME.Core.Recorder.Module
{
    public abstract class ObjectFrameDataRecorderModuleBase<TObject, TFrameData> : ObjectRecorderModuleBase<TObject>,
        IFrameDataRecorderModule where TObject : UnityEngine.Object where TFrameData : unmanaged, IFrameData
    {
        private readonly ConcurrentQueue<TFrameData> _frameDataQueue = new();

        void IFrameDataRecorderModule.EnqueueFrameData()
        {
            var frameData = CollectFrameData();
            ClearCreatedObjects();
            ClearDestroyedObjects();
            _frameDataQueue.Enqueue(frameData);
        }

        void IFrameDataRecorderModule.DequeueSerializedFrameData(SerializedSamplesBuffer buffer)
        {
            if(!_frameDataQueue.TryDequeue(out var frameData))
                throw new System.InvalidOperationException("No frame data to dequeue.");
            
            SerializeFrameData(frameData, buffer);
            
            // TODO: tester si le dispose fonctionne sur l'autre thread
            DisposeFrameData(frameData);
        }

        protected abstract TFrameData CollectFrameData();

        protected abstract void SerializeFrameData(TFrameData frameData, SerializedSamplesBuffer buffer);

        protected abstract void DisposeFrameData(TFrameData frameData);
    }
}