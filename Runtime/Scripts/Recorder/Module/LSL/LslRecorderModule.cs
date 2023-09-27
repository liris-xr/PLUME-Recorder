using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LSL;
using PLUME.Sample.LSL;
using UnityEngine;
using StreamInfo = PLUME.Sample.LSL.StreamInfo;

namespace PLUME
{
    public class LslRecorderModule : RecorderModule, IStartRecordingEventReceiver, IStopRecordingEventReceiver
    {
        [Tooltip("XPath predicate on stream")] public string resolverPredicate = "*";
        public float resolveInterval = .1f; // seconds
        public float forgetAfter = 5; // seconds

        private ContinuousResolver _streamResolver;
        private bool _resolveStream;

        // Stream ID should always start at 1 according to LSL code, otherwise it is considered unset if it equals 0.
        private uint _nextLslStreamId = 1;

        private readonly List<BufferedInlet> _recordedStreams = new();

        // Offset (in nanoseconds) between the LSL clock and PLUME clock
        private long _lslPlumeOffset;

        public uint PickNextLslStreamId()
        {
            return _nextLslStreamId++;
        }

        public void Awake()
        {
            _streamResolver = new ContinuousResolver(resolverPredicate, forgetAfter);
        }

        protected override void ResetCache()
        {
            _nextLslStreamId = 1;

            foreach (var recordedStream in _recordedStreams)
            {
                recordedStream.Close();
            }

            _recordedStreams.Clear();
        }

        public new void OnStartRecording()
        {
            base.OnStartRecording();
            _lslPlumeOffset = (long)recorder.Clock.GetTimeInNanoseconds() - (long)(Lsl.local_clock() * 1_000_000_000);
            StartCoroutine(ResolveStreams());
        }

        public void OnStopRecording()
        {
            _resolveStream = false;
            CloseAllStreams();
        }

        private void OnDestroy()
        {
            _resolveStream = false;
            CloseAllStreams();
        }

        private IEnumerator ResolveStreams()
        {
            _resolveStream = true;

            do
            {
                var lastKnownUIDs = new HashSet<string>(_recordedStreams.Select(s => s.Info().uid()));
                var lastKnownSourceIds = new HashSet<string>(_recordedStreams.Select(s => s.Info().source_id()));

                var forgottenStreamsByUid =
                    new Dictionary<string, BufferedInlet>(_recordedStreams.Select(s =>
                        new KeyValuePair<string, BufferedInlet>(s.Info().uid(), s)));

                foreach (var streamInfo in _streamResolver.results())
                {
                    if (!lastKnownUIDs.Contains(streamInfo.uid()))
                    {
                        if (string.IsNullOrEmpty(streamInfo.source_id()) ||
                            !lastKnownSourceIds.Contains(streamInfo.source_id()))
                        {
                            BufferedInlet bufferedInlet;

                            switch (streamInfo.channel_format())
                            {
                                case channel_format_t.cf_float32:
                                    bufferedInlet =
                                        new BufferedInlet<float>(streamInfo, PickNextLslStreamId());
                                    break;
                                case channel_format_t.cf_double64:
                                    bufferedInlet =
                                        new BufferedInlet<double>(streamInfo, PickNextLslStreamId());
                                    break;
                                case channel_format_t.cf_string:
                                    bufferedInlet =
                                        new BufferedInlet<string>(streamInfo, PickNextLslStreamId());
                                    break;
                                case channel_format_t.cf_int32:
                                    bufferedInlet = new BufferedInlet<int>(streamInfo, PickNextLslStreamId());
                                    break;
                                case channel_format_t.cf_int16:
                                    bufferedInlet =
                                        new BufferedInlet<short>(streamInfo, PickNextLslStreamId());
                                    break;
                                case channel_format_t.cf_int8:
                                    bufferedInlet =
                                        new BufferedInlet<sbyte>(streamInfo, PickNextLslStreamId());
                                    break;
                                case channel_format_t.cf_int64:
                                    bufferedInlet =
                                        new BufferedInlet<long>(streamInfo, PickNextLslStreamId());
                                    break;
                                case channel_format_t.cf_undefined:
                                default:
                                    throw new Exception($"Unsupported channel format {streamInfo.channel_format()}");
                            }

                            _recordedStreams.Add(bufferedInlet);
                            RecordOpenStream(bufferedInlet);
                        }
                    }

                    forgottenStreamsByUid.Remove(streamInfo.uid());
                }

                foreach (var (uid, forgottenStream) in forgottenStreamsByUid)
                {
                    var sourceId = forgottenStream.Info().source_id();

                    // If a stream with a different uid but identical source_id is found, assume that it was simply restarted
                    if (!string.IsNullOrEmpty(sourceId) &&
                        _recordedStreams.FirstOrDefault(s =>
                            s.Info().uid() != uid && s.Info().source_id() == sourceId) != null)
                    {
                        continue;
                    }

                    RecordCloseStream(forgottenStream);
                    _recordedStreams.Remove(forgottenStream);
                }

                yield return new WaitForSeconds(resolveInterval);
            } while (_resolveStream);
        }

        private void CloseAllStreams()
        {
            foreach (var stream in _recordedStreams)
            {
                RecordCloseStream(stream);
                stream.Close();
            }

            _recordedStreams.Clear();
        }

        private void Update()
        {
            foreach (var recordedStream in _recordedStreams)
            {
                SampleChunk chunk;
                
                while ((chunk = recordedStream.PullChunk()) != null)
                {
                    RecordStreamSampleChunk(recordedStream, chunk.Values, chunk.Timestamps);
                }
            }
        }

        private void RecordOpenStream(BufferedInlet inlet)
        {
            var plumeRawTimestamp = recorder.Clock.GetTimeInNanoseconds();
            var lslTimestamp = inlet.Info().created_at();
            var lslClockOffset = inlet.TimeCorrection();
            var correctedLslTimestamp = lslTimestamp + lslClockOffset;
            var correctedPlumeTimestamp = (long)(correctedLslTimestamp * 1_000_000_000 + _lslPlumeOffset);
            var plumeTimestampOffset = correctedPlumeTimestamp - (long)plumeRawTimestamp;
            
            var streamOpen = new StreamOpen();
            streamOpen.StreamInfo = new StreamInfo();
            streamOpen.StreamInfo.PlumeRawTimestamp = plumeRawTimestamp;
            streamOpen.StreamInfo.LslPlumeOffset = _lslPlumeOffset;
            streamOpen.StreamInfo.LslStreamId = inlet.StreamId.ToString();
            streamOpen.StreamInfo.LslTimestamp = lslTimestamp;
            streamOpen.StreamInfo.LslClockOffset = lslClockOffset;
            streamOpen.XmlHeader = inlet.Info().as_xml();

            recorder.RecordSampleStamped(streamOpen, plumeTimestampOffset);
        }

        private void RecordStreamSampleChunk(BufferedInlet inlet, ICollection<ICollection> samples,
            ICollection<double> timestamps)
        {
            for (var sampleIdx = 0; sampleIdx < samples.Count; ++sampleIdx)
            {
                var values = samples.ElementAt(sampleIdx);
                
                var streamSample = new StreamSample();

                switch (inlet.Info().channel_format())
                {
                    case channel_format_t.cf_float32:
                        streamSample.FloatValue = new RepeatedFloat();
                        streamSample.FloatValue.Value.AddRange(values as ICollection<float>);
                        break;
                    case channel_format_t.cf_double64:
                        streamSample.DoubleValue = new RepeatedDouble();
                        streamSample.DoubleValue.Value.AddRange(values as ICollection<double>);
                        break;
                    case channel_format_t.cf_string:
                        streamSample.StringValue = new RepeatedString();
                        streamSample.StringValue.Value.AddRange(values as ICollection<string>);
                        break;
                    case channel_format_t.cf_int32:
                        streamSample.FloatValue = new RepeatedFloat();
                        streamSample.FloatValue.Value.AddRange(values as ICollection<float>);
                        break;
                    case channel_format_t.cf_int8:
                        streamSample.Int8Value = new RepeatedInt32();
                        foreach (char val in values)
                        {
                            streamSample.Int8Value.Value.Add(val);
                        }

                        break;
                    case channel_format_t.cf_int16:
                        streamSample.Int16Value = new RepeatedInt32();

                        foreach (short val in values)
                        {
                            streamSample.Int16Value.Value.Add(val);
                        }

                        break;
                    case channel_format_t.cf_int64:
                        streamSample.Int64Value = new RepeatedInt64();
                        streamSample.Int64Value.Value.AddRange(values as ICollection<long>);
                        break;
                    case channel_format_t.cf_undefined:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var plumeRawTimestamp = recorder.Clock.GetTimeInNanoseconds();
                var lslTimestamp = timestamps.ElementAt(sampleIdx);
                var lslClockOffset = inlet.TimeCorrection();
                var correctedLslTimestamp = lslTimestamp + lslClockOffset;
                var correctedPlumeTimestamp = (long)(correctedLslTimestamp * 1_000_000_000 + _lslPlumeOffset);
                var plumeTimestampOffset = correctedPlumeTimestamp - (long)plumeRawTimestamp;
                streamSample.StreamInfo = new StreamInfo();
                streamSample.StreamInfo.PlumeRawTimestamp = plumeRawTimestamp;
                streamSample.StreamInfo.LslPlumeOffset = _lslPlumeOffset;
                streamSample.StreamInfo.LslStreamId = inlet.StreamId.ToString();
                streamSample.StreamInfo.LslTimestamp = lslTimestamp;
                streamSample.StreamInfo.LslClockOffset = lslClockOffset;

                // Discard any sample where the timestamp lands before t=0
                if ((long) plumeRawTimestamp + plumeTimestampOffset >= 0)
                {
                    recorder.RecordSampleStamped(streamSample, plumeTimestampOffset);
                }

                if (plumeTimestampOffset < -(long) RecordWriter.SampleWriteDelay)
                {
                    Debug.LogWarning("The LSL buffer is filling up quicker than it is being consumed.");
                }
            }
        }

        private void RecordCloseStream(BufferedInlet inlet)
        {
            var streamClose = new StreamClose();
            streamClose.StreamInfo = new StreamInfo();

            var plumeRawTimestamp = recorder.Clock.GetTimeInNanoseconds();
            var lslTimestamp = Lsl.local_clock();

            streamClose.StreamInfo.PlumeRawTimestamp = plumeRawTimestamp;
            streamClose.StreamInfo.LslPlumeOffset = _lslPlumeOffset;
            streamClose.StreamInfo.LslStreamId = inlet.StreamId.ToString();
            streamClose.StreamInfo.LslTimestamp = lslTimestamp;
            streamClose.StreamInfo.LslClockOffset = 0;
            recorder.RecordSampleStamped(streamClose);
        }
    }
}