using System.Collections.Generic;
using Google.Protobuf;
using ProtoBurst;
using Unity.Collections;

namespace PLUME.Core.Recorder.Data
{
    public interface IRecordData
    {
        public void PushTimelessSample(IMessage msg);

        public void PushTimestampedSample(IMessage msg, long timestamp);
        
        public void PushTimelessSample<T>(T msg) where T : unmanaged, IProtoBurstMessage;

        public void PushTimestampedSample<T>(T msg, long timestamp) where T : unmanaged, IProtoBurstMessage;

        public void PushTimelessSample(NativeArray<byte> sampleBytes, FixedString128Bytes sampleTypeUrl);

        public void PushTimestampedSample(NativeArray<byte> sampleBytes, FixedString128Bytes sampleTypeUrl,
            long timestamp);

        public bool TryPopAllTimelessDataChunks(DataChunks dataChunks);

        public bool TryPopAllTimestampedDataChunks(DataChunks dataChunks, List<long> timestamps);

        public bool TryPopTimestampedDataChunksBefore(long timestamp, DataChunks dataChunks, List<long> timestamps,
            bool inclusive);

        public void Clear();
    }
}