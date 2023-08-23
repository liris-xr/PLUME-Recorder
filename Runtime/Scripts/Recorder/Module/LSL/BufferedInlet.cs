using System;
using System.Collections;
using System.Collections.Generic;
using LSL;
using UnityEngine;

namespace PLUME
{
    public abstract class BufferedInlet
    {
        public readonly uint StreamId;
        protected readonly StreamInlet Inlet;
        protected ulong SampleCount;
        protected double FirstLslTimestamp = double.MaxValue;
        protected double LastLslTimestamp = double.MinValue;

        protected BufferedInlet(StreamInfo info, uint streamId)
        {
            StreamId = streamId;
            Inlet = new StreamInlet(info);
        }

        public abstract SampleChunk PullChunk();

        public double TimeCorrection()
        {
            return Inlet.time_correction();
        }
        
        public StreamInfo Info()
        {
            return Inlet.info();
        }

        public ulong GetSampleCount()
        {
            return SampleCount;
        }
        
        public double GetLslFirstTimestamp()
        {
            return FirstLslTimestamp;
        }
        
        public double GetLslLastTimestamp()
        {
            return LastLslTimestamp;
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
        private readonly T[,] _dataBuffer;
        private readonly double[] _timestampBuffer;

        /**
         * @param info Stream information such as source_id, uid, hostname, ...
         * @param maxChunkDuration Duration, in seconds, of buffer passed to pull_chunk. This must be > than average frame interval.
         */
        public BufferedInlet(StreamInfo info, uint streamId, double maxChunkDuration = 0.2) : base(info, streamId)
        {
            var bufSamples = (int) Mathf.Ceil((float) (info.nominal_srate() * maxChunkDuration));
            var nChannels = info.channel_count();
            _dataBuffer = new T[bufSamples, nChannels];
            _timestampBuffer = new double[bufSamples];
        }

        public override SampleChunk PullChunk()
        {
            try
            {
                var nPulledSamples = Inlet.pull_chunk(_dataBuffer, _timestampBuffer);
                var nChannels = Inlet.info().channel_count();

                if (nPulledSamples > 0)
                {
                    var values = new T[nPulledSamples][];
                    var timestamps = new double[nPulledSamples];

                    for (var sampleIdx = 0; sampleIdx < nPulledSamples; sampleIdx++)
                    {
                        FirstLslTimestamp = Math.Min(FirstLslTimestamp, _timestampBuffer[sampleIdx]);
                        LastLslTimestamp = Math.Max(LastLslTimestamp, _timestampBuffer[sampleIdx]);
                        
                        timestamps[sampleIdx] = _timestampBuffer[sampleIdx];
                        values[sampleIdx] = new T[nChannels];

                        for (var channelIdx = 0; channelIdx < nChannels; channelIdx++)
                        {
                            values[sampleIdx][channelIdx] = _dataBuffer[sampleIdx, channelIdx];
                        }
                    }

                    SampleCount += (ulong) nPulledSamples;

                    return new SampleChunk(values, timestamps);
                }

                return null;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return null;
            }
        }
    }

    public class SampleChunk
    {
        public readonly ICollection<ICollection> Values;
        public readonly ICollection<double> Timestamps;

        public SampleChunk(ICollection<ICollection> values, ICollection<double> timestamps)
        {
            Values = values;
            Timestamps = timestamps;
        }
    }
}