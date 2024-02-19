using System;
using Google.Protobuf;
using PLUME.Core.Recorder.Data;
using PLUME.Core.Recorder.ProtoBurst;
using PLUME.Core.Recorder.Time;
using ProtoBurst;
using ProtoBurst.Message;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Collections;
using Unity.Profiling;
using UnityEngine;

namespace PLUME.Core.Recorder
{
    public class Record : IDisposable
    {
        public IReadOnlyClock Clock => InternalClock;
        public readonly RecordIdentifier Identifier;
        
        internal readonly Clock InternalClock;
        
        private DataChunks _timelessDataBuffer;
        private TimestampedDataChunks _timestampedDataBuffer;
        private readonly object _timelessDataBufferLock = new();
        private readonly object _timestampedDataBufferLock = new();
        
        public static readonly ProfilerCounterValue<int> BufferSize = new(ProfilerCategory.Scripts, "Buffer size", ProfilerMarkerDataUnit.Count);
        public static readonly ProfilerCounterValue<int> FrameSize = new(ProfilerCategory.Scripts, "Frame size", ProfilerMarkerDataUnit.Count);
        
        internal Record(Clock clock, RecordIdentifier identifier, Allocator allocator)
        {
            InternalClock = clock;
            Identifier = identifier;
            _timelessDataBuffer = new DataChunks(allocator);
            _timestampedDataBuffer = new TimestampedDataChunks(allocator);
        }
        
        // public void RecordTimelessSample(IMessage msg)
        // {
        //     // var packedSample = PackedSample.Pack(msg, Allocator.Persistent);
        //     //
        //     // var bytes = packedSample.SerializeLengthPrefixed(Allocator.Persistent);
        //     //
        //     // lock (_timelessDataBufferLock)
        //     // {
        //     //     _timelessDataBuffer.Add(bytes.AsArray());
        //     // }
        //     //
        //     // bytes.Dispose();
        //     // packedSample.Dispose();
        // }
        //
        // public void RecordTimestampedSample(IMessage msg, long timestamp)
        // {
        //     // var packedSample = PackedSample.Pack(timestamp, msg, Allocator.Persistent);
        //     // var bytes = packedSample.SerializeLengthPrefixed(Allocator.Persistent);
        //     //
        //     // lock (_timestampedDataBufferLock)
        //     // {
        //     //     _timestampedDataBuffer.Add(bytes.AsArray(), timestamp);
        //     // }
        //     //
        //     // bytes.Dispose();
        //     // packedSample.Dispose();
        // }
        //
        // public void RecordTimelessSample<T>(T msg) where T : unmanaged, IProtoBurstMessage
        // {
        //     var bytes = new NativeList<byte>(PackedSample.ComputeSize(ref msg), Allocator.Persistent);
        //     var buffer = new BufferWriter(bytes);
        //     PackedSample.WriteTo(ref msg, ref buffer);
        //     
        //     lock (_timelessDataBufferLock)
        //     {
        //         _timelessDataBuffer.Add(bytes.AsArray());
        //     }
        //     
        //     bytes.Dispose();
        // }
        
        public void RecordTimestampedPackedSample(PackedSample sample, long timestamp)
        {
            var size = BufferExtensions.ComputeLengthPrefixedMessageSize(ref sample);
            var bytes = new NativeList<byte>(size, Allocator.Persistent);
            var buffer = new BufferWriter(bytes);
            
            buffer.WriteLengthPrefixedMessage(ref sample);
            
            lock (_timestampedDataBufferLock)
            {
                _timestampedDataBuffer.Add(bytes.AsArray(), timestamp);
                BufferSize.Value = _timestampedDataBuffer.DataLength;
                FrameSize.Value = bytes.Length;
            }
            
            bytes.Dispose();
        }

        // TODO: implement a blocking mechanism
        internal bool TryRemoveAllTimelessDataChunks(DataChunks dst)
        {
            lock (_timelessDataBufferLock)
            {
                return _timelessDataBuffer.TryRemoveAll(dst);
            }
        }

        internal bool TryRemoveAllTimestampedDataChunks(TimestampedDataChunks dst)
        {
            lock (_timestampedDataBufferLock)
            {
                BufferSize.Value = 0;
                return _timestampedDataBuffer.TryRemoveAll(dst);
            }
        }

        internal bool TryRemoveAllTimestampedDataChunksBeforeTimestamp(long timestamp, TimestampedDataChunks dst,
            bool inclusive)
        {
            lock (_timestampedDataBufferLock)
            {
                var rm = _timestampedDataBuffer.TryRemoveAllBeforeTimestamp(timestamp, dst, inclusive);
                BufferSize.Value = _timestampedDataBuffer.DataLength;
                return rm;
            }
        }

        public void Dispose()
        {
            _timelessDataBuffer.Dispose();
            _timestampedDataBuffer.Dispose();
        }
    }
}