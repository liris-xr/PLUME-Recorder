using System;

namespace PLUME.Core.Recorder.Module.Frame
{
    public abstract class PooledFrameData<T> : IFrameData, IDisposable where T : PooledFrameData<T>, new()
    {
        internal FrameDataPool<T> Pool;

        public void Dispose()
        {
            Pool.Release((T)this);
        }

        public abstract void Serialize(FrameDataWriter frameDataWriter);
        
        public abstract void Clear();
    }
}