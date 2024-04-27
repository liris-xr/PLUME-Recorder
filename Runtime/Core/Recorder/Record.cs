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
        public readonly RecordMetadata Metadata;

        /// <summary>
        /// Relative time since the recording started. This is the time that should be used for timestamping samples.
        /// </summary>
        public ulong Time => InternalClock.ElapsedNanoseconds;

        /// <summary>
        /// Relative time since the recording started. This is the time that should be used for timestamping samples inside fixed updates.
        /// </summary>
        public ulong FixedTime { get; internal set; }

        internal readonly Clock InternalClock;

        private DataChunks _timelessDataBuffer;
        private DataChunksTimestamped _dataChunksTimestampedDataBuffer;
        private readonly object _timelessDataBufferLock = new();
        private readonly object _timestampedDataBufferLock = new();

        internal Record(Clock clock, RecordMetadata metadata, Allocator allocator)
        {
            InternalClock = clock;
            Metadata = metadata;
            _timelessDataBuffer = new DataChunks(allocator);
            _dataChunksTimestampedDataBuffer = new DataChunksTimestamped(allocator);
        }

        // ReSharper restore Unity.ExpensiveCode

        public void RecordTimelessManagedSample(IMessage msg)
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

        public void RecordTimestampedManagedSample(IMessage msg)
        {
            RecordTimestampedManagedSample(msg, Time);
        }

        // ReSharper restore Unity.ExpensiveCode

        public void RecordTimestampedManagedSample(IMessage msg, ulong timestamp)
        {
            var typeUrl = SampleTypeUrl.Alloc(msg.Descriptor, Allocator.Persistent);
            var bytes = msg.ToByteArray();
            var sampleBytes = new NativeList<byte>(bytes.Length, Allocator.Persistent);

            unsafe
            {
                fixed (byte* bytesPtr = bytes)
                {
                    sampleBytes.AddRange(bytesPtr, bytes.Length);
                }
            }

            RecordTimestampedSample(sampleBytes, typeUrl, timestamp);
            typeUrl.Dispose();
            sampleBytes.Dispose();
        }
        
        public void RecordTimestampedSample(byte[] bytes, SampleTypeUrl typeUrl, ulong timestamp)
        {
            var sampleBytes = new NativeList<byte>(bytes.Length, Allocator.Persistent);

            unsafe
            {
                fixed (byte* bytesPtr = bytes)
                {
                    sampleBytes.AddRange(bytesPtr, bytes.Length);
                }
            }

            RecordTimestampedSample(sampleBytes, typeUrl, timestamp);
            sampleBytes.Dispose();
        }
        
        public void RecordTimestampedSample(Span<byte> bytes, SampleTypeUrl typeUrl, ulong timestamp)
        {
            var sampleBytes = new NativeList<byte>(bytes.Length, Allocator.Persistent);

            unsafe
            {
                fixed (byte* bytesPtr = bytes)
                {
                    sampleBytes.AddRange(bytesPtr, bytes.Length);
                }
            }

            RecordTimestampedSample(sampleBytes, typeUrl, timestamp);
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

        public void RecordTimestampedSample<T>(T msg, ulong timestamp) where T : unmanaged, IProtoBurstMessage
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

        public void RecordTimestampedSample(NativeList<byte> sampleBytes, SampleTypeUrl typeUrl, ulong timestamp)
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

        internal bool TryRemoveAllTimestampedDataChunksBeforeTimestamp(ulong timestamp, DataChunksTimestamped dst,
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