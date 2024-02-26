using System;
using Google.Protobuf;
using PLUME.Sample.ProtoBurst;
using ProtoBurst;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Collections;

namespace PLUME.Core.Recorder
{
    public class Record : IDisposable
    {
        public readonly RecordIdentifier Identifier;
        
        public long Time => InternalClock.ElapsedNanoseconds;
        public long FixedTime { get; internal set; }
        
        internal readonly Clock InternalClock;

        private DataChunks _timelessDataBuffer;
        private DataChunksTimestamped _dataChunksTimestampedDataBuffer;
        private readonly object _timelessDataBufferLock = new();
        private readonly object _timestampedDataBufferLock = new();

        internal Record(Clock clock, RecordIdentifier identifier, Allocator allocator)
        {
            InternalClock = clock;
            Identifier = identifier;
            _timelessDataBuffer = new DataChunks(allocator);
            _dataChunksTimestampedDataBuffer = new DataChunksTimestamped(allocator);
        }

        // ReSharper restore Unity.ExpensiveCode

        public void RecordTimelessPackedSample(IMessage msg)
        {
            var typeUrl = SampleTypeUrl.Alloc(msg.Descriptor, Allocator.Persistent);
            var sampleSize = msg.CalculateSize();
            var sampleBytes = new NativeList<byte>(sampleSize, Allocator.Persistent);
            var bytes = msg.ToByteArray();

            unsafe
            {
                fixed (byte* bytesPtr = bytes)
                {
                    sampleBytes.AddRange(bytesPtr, sampleSize);
                }
            }
            
            RecordTimelessSample(sampleBytes, typeUrl);
            typeUrl.Dispose();
            sampleBytes.Dispose();
        }

        // ReSharper restore Unity.ExpensiveCode

        public void RecordTimestampedSample(IMessage msg)
        {
            RecordTimestampedSample(msg, Time);
        }
        
        // ReSharper restore Unity.ExpensiveCode

        public void RecordTimestampedSample(IMessage msg, long timestamp)
        {
            var typeUrl = SampleTypeUrl.Alloc(msg.Descriptor, Allocator.Persistent);
            var sampleSize = msg.CalculateSize();
            var sampleBytes = new NativeList<byte>(sampleSize, Allocator.Persistent);
            var bytes = msg.ToByteArray();

            unsafe
            {
                fixed (byte* bytesPtr = bytes)
                {
                    sampleBytes.AddRange(bytesPtr, sampleSize);
                }
            }
            
            RecordTimestampedSample(sampleBytes, typeUrl, timestamp);
            typeUrl.Dispose();
            sampleBytes.Dispose();
        }

        public void RecordTimelessSample<T>(T msg) where T : unmanaged, IProtoBurstMessage
        {
            var typeUrl = msg.GetTypeUrl(Allocator.Persistent);
            var sampleBytes = msg.ToBytes(Allocator.Persistent);
            RecordTimelessSample(sampleBytes, typeUrl);
            typeUrl.Dispose();
            sampleBytes.Dispose();
        }

        public void RecordTimestampedSample<T>(T msg) where T : unmanaged, IProtoBurstMessage
        {
            RecordTimestampedSample(msg, Time);
        }

        public void RecordTimestampedSample<T>(T msg, long timestamp) where T : unmanaged, IProtoBurstMessage
        {
            var typeUrl = msg.GetTypeUrl(Allocator.Persistent);
            var sampleBytes = msg.ToBytes(Allocator.Persistent);
            RecordTimestampedSample(sampleBytes, typeUrl, timestamp);
            typeUrl.Dispose();
            sampleBytes.Dispose();
        }

        public void RecordTimelessSample(NativeList<byte> sampleBytes, SampleTypeUrl typeUrl)
        {
            var typeUrlBytes = typeUrl.AsList();
            var packedSample = PackedSample.Pack(ref sampleBytes, ref typeUrlBytes, Allocator.Persistent);

            var size = packedSample.ComputeSize();
            var packedSize = size + BufferWriterExtensions.ComputeLengthPrefixSize(size);

            var bytes = new NativeList<byte>(packedSize, Allocator.Persistent);
            var buffer = new BufferWriter(bytes);

            buffer.WriteLength(size);
            packedSample.WriteTo(ref buffer);

            lock (_timelessDataBufferLock)
            {
                _timelessDataBuffer.Add(bytes.AsArray().AsReadOnlySpan());
            }

            packedSample.Dispose();
            bytes.Dispose();
        }

        public void RecordTimestampedSample(NativeList<byte> sampleBytes, SampleTypeUrl typeUrl)
        {
            RecordTimestampedSample(sampleBytes, typeUrl, Time);
        }

        public void RecordTimestampedSample(NativeList<byte> sampleBytes, SampleTypeUrl typeUrl, long timestamp)
        {
            var typeUrlBytes = typeUrl.AsList();
            var packedSample = PackedSample.Pack(timestamp, ref sampleBytes, ref typeUrlBytes, Allocator.Persistent);

            var size = packedSample.ComputeSize();
            var packedSize = size + BufferWriterExtensions.ComputeLengthPrefixSize(size);

            var bytes = new NativeList<byte>(packedSize, Allocator.Persistent);
            var buffer = new BufferWriter(bytes);

            buffer.WriteLength(size);
            packedSample.WriteTo(ref buffer);

            lock (_timestampedDataBufferLock)
            {
                _dataChunksTimestampedDataBuffer.Add(bytes.AsArray().AsReadOnlySpan(), timestamp);
            }

            packedSample.Dispose();
            bytes.Dispose();
        }

        internal bool TryRemoveAllTimelessDataChunks(DataChunks dst)
        {
            lock (_timelessDataBufferLock)
            {
                return _timelessDataBuffer.TryRemoveAll(dst);
            }
        }

        internal bool TryRemoveAllTimestampedDataChunks(DataChunksTimestamped dst)
        {
            lock (_timestampedDataBufferLock)
            {
                return _dataChunksTimestampedDataBuffer.TryRemoveAll(dst);
            }
        }

        internal bool TryRemoveAllTimestampedDataChunksBeforeTimestamp(long timestamp, DataChunksTimestamped dst,
            bool inclusive)
        {
            lock (_timestampedDataBufferLock)
            {
                return _dataChunksTimestampedDataBuffer.TryRemoveAllBeforeTimestamp(timestamp, dst, inclusive);
            }
        }

        public void Dispose()
        {
            _timelessDataBuffer.Dispose();
            _dataChunksTimestampedDataBuffer.Dispose();
        }
    }
}