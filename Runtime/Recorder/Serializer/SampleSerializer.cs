using System;
using System.Collections.Generic;
using PLUME.Recorder.Module;

namespace PLUME.Recorder.Serializer
{
    public abstract class SampleSerializer
    {
        public abstract Type SupportedSampleType { get; }

        public abstract void Serialize(IUnityObjectState unityObjectState, IByteBufferWriter bufferWriter);

        public abstract void SerializeBatch(IEnumerable<IUnityObjectState> samples, IByteBufferWriter bufferWriter);
    }

    public abstract class SampleSerializer<T> : SampleSerializer where T : IUnityObjectState
    {
        public override Type SupportedSampleType => typeof(T);

        public override void Serialize(IUnityObjectState unityObjectState, IByteBufferWriter bufferWriter)
        {
            if (unityObjectState is T typedSample)
                Serialize(typedSample, bufferWriter);
            else
                throw new InvalidCastException(
                    $"Cannot serialize sample of type {unityObjectState.GetType().Name} as {typeof(T).Name}");
        }

        public override void SerializeBatch(IEnumerable<IUnityObjectState> samples, IByteBufferWriter bufferWriter)
        {
            if (samples is IEnumerable<T> typedSamples)
                SerializeBatch(typedSamples, bufferWriter);
            else
                throw new InvalidCastException(
                    $"Cannot serialize samples of type {samples.GetType().Name} as {typeof(T).Name}");
        }

        public abstract void Serialize(T sample, IByteBufferWriter bufferWriter);

        public abstract void SerializeBatch(IEnumerable<T> samples, IByteBufferWriter bufferWriter);
    }
}