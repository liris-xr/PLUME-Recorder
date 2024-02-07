using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PLUME.Recorder.Serializer;
using UnityEngine;

namespace PLUME.Recorder
{
    public class UnityFrameSerializer
    {
        private readonly Dictionary<Type, TimestampedSampleSerializer> _sampleSerializerMap;

        public UnityFrameSerializer(IEnumerable<TimestampedSampleSerializer> sampleSerializers)
        {
            _sampleSerializerMap = new Dictionary<Type, TimestampedSampleSerializer>();

            foreach (var sampleSerializer in sampleSerializers)
            {
                if (_sampleSerializerMap.TryGetValue(sampleSerializer.SupportedSampleType, out var serializer))
                {
                    Debug.LogWarning($@"Trying to register serializer {sampleSerializer} for type
                                      {sampleSerializer.SupportedSampleType.Name} but {serializer.GetType().Name}
                                     is already registered for this type. Ignoring.");
                    continue;
                }

                _sampleSerializerMap[sampleSerializer.SupportedSampleType] = sampleSerializer;
            }
        }

        public void SerializeFrame(FrameData frameData, IByteBufferWriter byteBufferWriter)
        {
            foreach (var (sampleType, frameSamplesList) in frameData.SamplesByType)
            {
                if (!_sampleSerializerMap.TryGetValue(sampleType, out var serializer))
                {
                    Debug.LogWarning($"No serializer found for type {sampleType.Name}. Ignoring.");
                    continue;
                }

                serializer.SerializeBatch(frameSamplesList, frameData.Timestamp, byteBufferWriter);
            }
        }

        public async UniTask SerializeFrameAsync(FrameData frameData, IByteBufferWriter byteBufferWriter)
        {
            foreach (var (sampleType, frameSamplesList) in frameData.SamplesByType)
            {
                if (!_sampleSerializerMap.TryGetValue(sampleType, out var serializer))
                {
                    Debug.LogWarning($"No serializer found for type {sampleType.Name}. Ignoring.");
                    continue;
                }

                await serializer.SerializeBatchAsync(frameSamplesList, frameData.Timestamp, byteBufferWriter);
            }
        }
    }
}