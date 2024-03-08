using System;
using UnityEngine;

namespace PLUME.Base.Module.LSL
{
    public abstract class BufferedInlet
    {
        public readonly string StreamId;
        public readonly string Name;
        public readonly int ChannelCount;
        public readonly double NominalSampleRate;
        public readonly channel_format_t ChannelFormat;
        public readonly string Uid;
        public readonly string SourceId;
        public readonly double CreatedAt;
        public readonly string InfoXml;
        protected readonly StreamInlet Inlet;

        protected BufferedInlet(StreamInfo info, uint streamId)
        {
            StreamId = streamId.ToString();
            Inlet = new StreamInlet(info);
            Name = info.name();
            ChannelCount = info.channel_count();
            NominalSampleRate = info.nominal_srate();
            ChannelFormat = info.channel_format();
            Uid = info.uid();
            SourceId = info.source_id();
            CreatedAt = info.created_at();
            InfoXml = info.as_xml();
        }

        public abstract int PullChunk();

        public abstract Array GetDataBuffer();

        public abstract double[] GetTimestampBuffer();

        public double TimeCorrection()
        {
            return Inlet.time_correction();
        }

        public void Close()
        {
            Inlet.close_stream();
        }

        public void Dispose()
        {
            Inlet.close_stream();
        }
    }

    public class BufferedInlet<T> : BufferedInlet, IDisposable
    {
        // Flattened buffer for all channels values interleaved by sample.
        private readonly T[] _dataBuffer;
        // Timestamps buffer for each pulled sample.
        private readonly double[] _timestampBuffer;

        /**
         * @param info Stream information such as source_id, uid, hostname, ...
         * @param maxChunkDuration Duration, in seconds, of buffer passed to pull_chunk. This must be > than average frame interval.
         */
        public BufferedInlet(StreamInfo info, uint streamId, double maxChunkDuration = 0.2) : base(info, streamId)
        {
            var bufSamples = (int)Mathf.Ceil((float)(info.nominal_srate() * maxChunkDuration));
            var nChannels = info.channel_count();
            _dataBuffer = new T[bufSamples * nChannels];
            _timestampBuffer = new double[bufSamples];
        }

        /// <summary>
        /// Pull a chunk of data from the inlet into the buffer.
        /// </summary>
        /// <returns>
        /// The number of samples actually pulled. This can be less than the buffer size (in number of samples) if there was not enough data in the inlet to satisfy the chunk request.
        /// </returns>
        public override int PullChunk()
        {
            try
            {
                return Inlet.pull_chunk(_dataBuffer, _timestampBuffer, ChannelCount);
            }
            catch (LostException)
            {
                return 0;
            }
        }

        public override Array GetDataBuffer()
        {
            return _dataBuffer;
        }

        public override double[] GetTimestampBuffer()
        {
            return _timestampBuffer;
        }
    }
}