using System;
using System.Collections.Generic;
using Google.Protobuf;
using PLUME.Sample;

namespace PLUME
{
    public class SamplePoolManager
    {
        private readonly ThreadSafeObjectPool<PackedSample> _sampleStampedPool;
        private readonly ThreadSafeObjectPool<PackedSample> _samplePool;
        private readonly ThreadSafeObjectPool<UnpackedSample> _unpackedSampleStampedPool;
        private readonly ThreadSafeObjectPool<UnpackedSample> _unpackedSamplePool;

        private readonly Dictionary<Type, SamplePayloadPool> _samplePayloadPools;

        public SamplePoolManager()
        {
            _samplePayloadPools = new Dictionary<Type, SamplePayloadPool>();

            _sampleStampedPool = new ThreadSafeObjectPool<PackedSample>(() =>
            {
                var sampleStamped = new PackedSample();
                sampleStamped.Header = new SampleHeader();
                return sampleStamped;
            });
            
            _samplePool = new ThreadSafeObjectPool<PackedSample>(() =>
            {
                var sampleStamped = new PackedSample();
                return sampleStamped;
            });
            
            _unpackedSampleStampedPool = new ThreadSafeObjectPool<UnpackedSample>(() =>
            {
                var unpackedSampleStamped = new UnpackedSample();
                unpackedSampleStamped.Header = new SampleHeader();
                return unpackedSampleStamped;
            });
            
            _unpackedSamplePool = new ThreadSafeObjectPool<UnpackedSample>(() =>
            {
                var unpackedSampleStamped = new UnpackedSample();
                return unpackedSampleStamped;
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
        
        public UnpackedSample GetUnpackedSampleStamped()
        {
            return _unpackedSampleStampedPool.Get();
        }
        
        public PackedSample GetPackedSample()
        {
            return _samplePool.Get();
        }
        
        public PackedSample GetPackedSampleStamped()
        {
            return _sampleStampedPool.Get();
        }
        
        public void ReleasePackedSample(PackedSample sample)
        {
            sample.Header = null;
            _samplePool.Release(sample);
        }

        public void ReleasePackedSampleStamped(PackedSample sample)
        {
            sample.Header ??= new SampleHeader();
            _sampleStampedPool.Release(sample);
        }
        
        public void ReleaseUnpackedSample(UnpackedSample unpackedSample)
        {
            unpackedSample.Header = null;
            _unpackedSamplePool.Release(unpackedSample);
        }
        
        public void ReleaseUnpackedSampleStamped(UnpackedSample unpackedSample)
        {
            unpackedSample.Header ??= new SampleHeader();
            _unpackedSampleStampedPool.Release(unpackedSample);
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