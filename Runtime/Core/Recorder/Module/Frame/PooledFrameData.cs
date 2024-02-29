using System;

namespace PLUME.Core.Recorder.Module.Frame
{
    public abstract class PooledFrameData<T> : IFrameData, IDisposable where T : PooledFrameData<T>, new()
    {
        internal FrameDataPool<T> PoolInternal;

        public void Dispose()
        {
            if (this is T t && PoolInternal != null)
            {
                PoolInternal.Release(t);
            }
        }

        public abstract void Serialize(FrameDataWriter frameDataWriter);
        
        public abstract void Clear();
    }
}