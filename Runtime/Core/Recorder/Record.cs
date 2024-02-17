using System;
using Google.Protobuf;
using PLUME.Core.Recorder.Data;
using PLUME.Core.Recorder.Time;
using ProtoBurst;

namespace PLUME.Core.Recorder
{
    public class Record : IDisposable
    {
        public IReadOnlyClock Clock => InternalClock;
        
        public readonly RecordIdentifier Identifier;
        
        internal readonly Clock InternalClock;
        
        internal readonly RecordDataBuffer DataBuffer;

        internal Record(Clock clock, RecordDataBuffer dataBuffer, RecordIdentifier identifier)
        {
            InternalClock = clock;
            DataBuffer = dataBuffer;
            Identifier = identifier;
        }
        
        public void RecordTimelessSample<T>(T sample) where T : unmanaged, IProtoBurstMessage
        {
            DataBuffer.AddTimelessSample(sample);
        }
        
        public void RecordTimelessSample(IMessage sample)
        {
            DataBuffer.AddTimelessSample(sample);
        }
        
        public void RecordTimestampedSample<T>(T sample, long timestamp) where T : unmanaged, IProtoBurstMessage
        {
            DataBuffer.AddTimestampedSample(sample, timestamp);
        }
        
        public void RecordTimestampedSample(IMessage sample, long timestamp)
        {
            DataBuffer.AddTimestampedSample(sample, timestamp);
        }
        
        public void Dispose()
        {
            DataBuffer.Dispose();
        }
    }
}