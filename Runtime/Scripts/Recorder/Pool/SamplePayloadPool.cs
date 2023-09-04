using System;
using System.Collections.Generic;
using Google.Protobuf;

namespace PLUME
{
    public class SamplePayloadPool<T> : SamplePayloadPool where T : class, IMessage
    {
        public SamplePayloadPool(Func<T> createFunc, int defaultCapacity = 10, int maxSize = 10000) : base(createFunc,
            defaultCapacity, maxSize)
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

    public abstract class SamplePayloadPool : ThreadSafeObjectPool<IMessage>
    {
        protected SamplePayloadPool(Func<IMessage> createFunc, int defaultCapacity = 10, int maxSize = 10000) : base(
            createFunc, defaultCapacity, maxSize)
        {
        }
    }
}