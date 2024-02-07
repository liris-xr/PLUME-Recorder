using System;
using Cysharp.Threading.Tasks;
using PLUME.Recorder.Module;

namespace PLUME.Recorder.Serializer
{
    public abstract class TimestampedSampleSerializer
    {
        public abstract Type SupportedSampleType { get; }

        internal abstract void Serialize(IUnityObjectState unityObjectState, long timestamp, IByteBufferWriter bufferWriter);

        internal abstract void SerializeBatch(IStateCollection samples, long timestamp, IByteBufferWriter bufferWriter);

        internal abstract UniTask SerializeAsync(IUnityObjectState unityObjectState, long timestamp, IByteBufferWriter bufferWriter);

        internal abstract UniTask SerializeBatchAsync(IStateCollection samples, long timestamp, IByteBufferWriter bufferWriter);
    }

    public abstract class TimestampedSampleSerializer<TSample> : TimestampedSampleSerializer where TSample : IUnityObjectState
    {
        public override Type SupportedSampleType => typeof(TSample);

        internal override void Serialize(IUnityObjectState unityObjectState, long timestamp, IByteBufferWriter bufferWriter)
        {
            if (unityObjectState is TSample typedSample)
                Serialize(typedSample, timestamp, bufferWriter);
            else
                throw new InvalidCastException(
                    $"Cannot serialize sample of type {unityObjectState.GetType().Name} as {typeof(TSample).Name}");
        }

        internal override void SerializeBatch(IStateCollection samples, long timestamp, IByteBufferWriter bufferWriter)
        {
            if (samples is StateCollection<TSample> typedSamples)
                SerializeBatch(typedSamples, timestamp, bufferWriter);
            else
                throw new InvalidCastException(
                    $"Cannot serialize samples of enumerable {samples.GetType().Name} as {typeof(TSample).Name}");
        }

        internal override UniTask SerializeAsync(IUnityObjectState unityObjectState, long timestamp, IByteBufferWriter bufferWriter)
        {
            if (unityObjectState is TSample typedSample)
                return SerializeAsync(typedSample, timestamp, bufferWriter);
            throw new InvalidCastException(
                $"Cannot serialize sample of type {unityObjectState.GetType().Name} as {typeof(TSample).Name}");
        }

        internal override UniTask SerializeBatchAsync(IStateCollection samples, long timestamp, IByteBufferWriter bufferWriter)
        {
            if (samples is StateCollection<TSample> typedSamples)
                return SerializeBatchAsync(typedSamples, timestamp, bufferWriter);
            throw new InvalidCastException(
                $"Cannot serialize samples of enumerable {samples.GetType().Name} as {typeof(TSample).Name}");
        }

        public abstract void SerializeBatch(StateCollection<TSample> samples, long timestamp, IByteBufferWriter bufferWriter);

        public abstract UniTask SerializeBatchAsync(StateCollection<TSample> samples, long timestamp,
            IByteBufferWriter bufferWriter);

        public abstract void Serialize(TSample sample, long timestamp, IByteBufferWriter bufferWriter);

        public abstract UniTask SerializeAsync(TSample sample, long timestamp, IByteBufferWriter bufferWriter);
    }
}