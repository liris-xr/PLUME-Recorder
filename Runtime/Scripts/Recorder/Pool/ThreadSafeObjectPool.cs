using System;
using System.Collections.Generic;
using System.Threading;

namespace PLUME
{
    public class ThreadSafeObjectPool<T> : ThreadSafeObjectPool where T : class
    {
        public ThreadSafeObjectPool(Func<T> createFunc, int defaultCapacity = 10, int maxSize = 10000) : base(
            createFunc, defaultCapacity, maxSize)
        {
        }

        public new T Get()
        {
            return (T) base.Get();
        }

        public void Release(T element)
        {
            base.Release(element);
        }
    }

    public abstract class ThreadSafeObjectPool : IDisposable
    {
        private readonly object _lock = new();

        private readonly Stack<object> _pooledObjects;
        private readonly Func<object> _createFunc;
        private readonly int _maxSize;

        public int CountAll { get; private set; }
        public int CountActive => CountAll - CountInactive;

        public int CountInactive
        {
            get
            {
                lock (_lock)
                {
                    return _pooledObjects.Count;
                }
            }
        }

        protected ThreadSafeObjectPool(Func<object> createFunc, int defaultCapacity = 10, int maxSize = 10000)
        {
            if (maxSize <= 0)
                throw new ArgumentException("Max Size must be greater than 0", nameof(maxSize));
            _createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
            _pooledObjects = new Stack<object>(defaultCapacity);
            _maxSize = maxSize;
        }

        protected object Get()
        {
            lock (_lock)
            {
                return UnsafeGet();
            }
        }

        protected object UnsafeGet()
        {
            object obj;
            if (_pooledObjects.Count == 0)
            {
                obj = _createFunc();
                ++CountAll;
            }
            else
            {
                obj = _pooledObjects.Pop();
            }

            return obj;
        }

        protected void Release(object element)
        {
            lock (_lock)
            {
                UnsafeRelease(element);
            }
        }

        protected void UnsafeRelease(object element)
        {
            if (_pooledObjects.Count < _maxSize)
            {
                _pooledObjects.Push(element);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _pooledObjects.Clear();
                CountAll = 0;
            }
        }

        public void Dispose() => Clear();
    }
}