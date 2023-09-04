using System;
using System.Collections.Generic;
using Google.Protobuf;
using PLUME.Sample;

namespace PLUME
{
    public class SamplePoolManager
    {
        private readonly ThreadSafeObjectPool<UnpackedSample> _unpackedSamplePool;

        private readonly Dictionary<Type, SamplePayloadPool> _samplePayloadPools;

        public SamplePoolManager()
        {
            _samplePayloadPools = new Dictionary<Type, SamplePayloadPool>();

            _unpackedSamplePool = new ThreadSafeObjectPool<UnpackedSample>(() =>
            {
                var unpackedSample = new UnpackedSample();
                unpackedSample.Header = new SampleHeader();
                return unpackedSample;
            });
        }

        public SamplePayloadPool<T> CreateSamplePayloadPool<T>(Func<T> createFunc, int defaultCapacity = 10,
            int maxSize = 10000) where T : class, IMessage<T>
        {
            var type = typeof(T);

            if (_samplePayloadPools.ContainsKey(type))
                throw new Exception($"Sample pool for type {type} already exists.");

            var pool = new SamplePayloadPool<T>(createFunc, defaultCapacity, maxSize);
            _samplePayloadPools.Add(typeof(T), pool);
            return pool;
        }

        public UnpackedSample GetUnpackedSample()
        {
            return _unpackedSamplePool.Get();
        }

        public void ReleaseUnpackedSample(UnpackedSample unpackedSample)
        {
            _unpackedSamplePool.Release(unpackedSample);
        }

        public void ReleaseSamplePayload(IMessage sample)
        {
            if (_samplePayloadPools.TryGetValue(sample.GetType(), out var samplePayloadPool))
            {
                samplePayloadPool.Release(sample);
            }
        }

        public T GetSamplePayload<T>() where T : class, IMessage
        {
            if (_samplePayloadPools.TryGetValue(typeof(T), out var samplePayloadPool))
            {
                var pool = samplePayloadPool as ThreadSafeObjectPool<T>;
                return pool.Get();
            }

            return Activator.CreateInstance<T>();
        }
    }
}