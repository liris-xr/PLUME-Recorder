using System;
using UnityEngine.Pool;

namespace PLUME.Core.Recorder.Module.Frame
{
    public class FrameDataPool<T> : IDisposable where T : PooledFrameData<T>, new()
    {
        private readonly ObjectPool<T> _pool;

        public FrameDataPool(int defaultCapacity = 10, int maxSize = 10000)
        {
            _pool = new ObjectPool<T>(
                () =>
                {
                    var pooledFrameData = new T();
                    pooledFrameData.Pool = this;
                    return pooledFrameData;
                },
                pooledFrameData => { pooledFrameData.Clear(); }, null, null, false, defaultCapacity, maxSize);
        }

        public T Get()
        {
            lock (_pool)
            {
                return _pool.Get();
            }
        }

        public void Release(T obj)
        {
            lock (_pool)
            {
                _pool.Release(obj);
            }
        }

        public void Dispose()
        {
            lock (_pool)
            {
                _pool.Dispose();
            }
        }
    }
}