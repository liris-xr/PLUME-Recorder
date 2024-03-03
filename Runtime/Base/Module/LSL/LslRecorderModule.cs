using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PLUME.Base.Settings;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module;
using PLUME.Sample.LSL;
using UnityEngine.Scripting;

namespace PLUME.Base.Module.LSL
{
    [Preserve]
    public class LslRecorderModule : RecorderModule
    {
        private LslRecorderModuleSettings _settings;

        private readonly List<BufferedInlet> _resolvedStreams = new();
        private Thread _streamResolverThread;
        private Thread _streamRecorderThread;

        // Offset (in nanoseconds) between the LSL clock and the recorder clock
        private long _lslPlumeOffset;

        protected override void OnCreate(RecorderContext ctx)
        {
            base.OnCreate(ctx);
            _settings = ctx.SettingsProvider.GetOrCreate<LslRecorderModuleSettings>();
        }

        protected override void OnStartRecording(RecorderContext ctx)
        {
            base.OnStartRecording(ctx);
            _lslPlumeOffset = (long)ctx.CurrentRecord.Time - (long)(Lsl.local_clock() * 1_000_000_000);

            _streamResolverThread = new Thread(() => LslStreamResolverLoop(ctx))
            {
                Name = "LSL Stream Resolver"
            };
            _streamResolverThread.Start();

            _streamRecorderThread = new Thread(() => LslStreamRecorderLoop(ctx))
            {
                Name = "LSL Stream Recorder"
            };
            _streamRecorderThread.Start();
        }

        protected override void OnStopRecording(RecorderContext ctx)
        {
            base.OnStopRecording(ctx);
            _streamResolverThread.Join();
            _streamRecorderThread.Join();
            
            lock (_resolvedStreams)
            {
                _resolvedStreams.Clear();
            }
        }

        private void LslStreamRecorderLoop(RecorderContext ctx)
        {
            while (ctx.IsRecording)
            {
                lock (_resolvedStreams)
                {
                    foreach (var recordedStream in _resolvedStreams)
                    {
                        SampleChunk chunk;

                        do
                        {
                            chunk = recordedStream.PullChunk();

                            if (chunk != null)
                            {
                                RecordStreamSampleChunk(recordedStream, chunk.Values, chunk.Timestamps, ctx);
                            }
                        } while (chunk != null);
                    }
                }
            }
        }

        private void LslStreamResolverLoop(RecorderContext ctx)
        {
            var streamResolver = new ContinuousResolver(_settings.ResolverPredicate, _settings.ForgetAfter);

            // Stream ID should always start at 1 according to LSL documentation.
            // Otherwise the id is considered unset if it equals 0.
            var nextLslStreamId = 1u;

            var lastKnownUIDs = new HashSet<string>();
            var lastKnownSourceIds = new HashSet<string>();
            var forgottenStreamsByUid = new Dictionary<string, BufferedInlet>();

            while (ctx.IsRecording)
            {
                lastKnownUIDs.Clear();
                lastKnownSourceIds.Clear();
                forgottenStreamsByUid.Clear();

                lock (_resolvedStreams)
                {
                    foreach (var resolvedStream in _resolvedStreams)
                    {
                        lastKnownUIDs.Add(resolvedStream.Info().uid());
                        lastKnownSourceIds.Add(resolvedStream.Info().source_id());
                        forgottenStreamsByUid.Add(resolvedStream.Info().uid(), resolvedStream);
                    }
                }

                foreach (var streamInfo in streamResolver.results())
                {
                    var streamSourceId = streamInfo.source_id();
                    var streamUid = streamInfo.uid();
                    var channelFormat = streamInfo.channel_format();

                    forgottenStreamsByUid.Remove(streamInfo.uid());

                    if (lastKnownUIDs.Contains(streamUid))
                        continue;

                    if (lastKnownSourceIds.Contains(streamSourceId) && !string.IsNullOrEmpty(streamSourceId))
                        continue;

                    BufferedInlet bufferedInlet = channelFormat switch
                    {
                        channel_format_t.cf_float32 => new BufferedInlet<float>(streamInfo, nextLslStreamId++),
                        channel_format_t.cf_double64 => new BufferedInlet<double>(streamInfo, nextLslStreamId++),
                        channel_format_t.cf_string => new BufferedInlet<string>(streamInfo, nextLslStreamId++),
                        channel_format_t.cf_int32 => new BufferedInlet<int>(streamInfo, nextLslStreamId++),
                        channel_format_t.cf_int16 => new BufferedInlet<short>(streamInfo, nextLslStreamId++),
                        channel_format_t.cf_int8 => new BufferedInlet<sbyte>(streamInfo, nextLslStreamId++),
                        channel_format_t.cf_int64 => new BufferedInlet<long>(streamInfo, nextLslStreamId++),
                        channel_format_t.cf_undefined => throw new Exception(
                            $"Unsupported channel format {channelFormat}"),
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    RecordOpenStream(bufferedInlet, ctx);

                    lock (_resolvedStreams)
                    {
                        _resolvedStreams.Add(bufferedInlet);
                    }
                }

                foreach (var (uid, forgottenStream) in forgottenStreamsByUid)
                {
                    var sourceId = forgottenStream.Info().source_id();

                    lock (_resolvedStreams)
                    {
                        if (!string.IsNullOrEmpty(sourceId) &&
                            _resolvedStreams.FirstOrDefault(s =>
                                s.Info().uid() != uid && s.Info().source_id() == sourceId) != null)
                        {
                            continue;
                        }

                        lock (_resolvedStreams)
                        {
                            _resolvedStreams.Remove(forgottenStream);
                        }
                    }

                    RecordCloseStream(forgottenStream, ctx);
                }

                Thread.Sleep(_settings.ResolveInterval);
            }
        }

        private void RecordOpenStream(BufferedInlet inlet, RecorderContext ctx)
        {
            var plumeRawTimestamp = ctx.CurrentRecord.Time;
            var lslTimestamp = inlet.Info().created_at();
            var lslClockOffset = inlet.TimeCorrection();
            var correctedLslTimestamp = lslTimestamp + lslClockOffset;
            var correctedPlumeTimestamp = (long)(correctedLslTimestamp * 1_000_000_000 + _lslPlumeOffset);

            if (correctedPlumeTimestamp < 0) return;

            var streamOpen = new StreamOpen
            {
                StreamInfo = new Sample.LSL.StreamInfo
                {
                    PlumeRawTimestamp = plumeRawTimestamp,
                    LslPlumeOffset = _lslPlumeOffset,
                    LslStreamId = inlet.StreamId.ToString(),
                    LslTimestamp = lslTimestamp,
                    LslClockOffset = lslClockOffset
                },
                XmlHeader = inlet.Info().as_xml()
            };

            ctx.CurrentRecord.RecordTimestampedManagedSample(streamOpen, (ulong)correctedPlumeTimestamp);
        }

        private void RecordCloseStream(BufferedInlet inlet, RecorderContext ctx)
        {
            // We can't query the inlet time correction when closed, we directly use the recorder timestamp.
            var plumeTimestamp = ctx.CurrentRecord.Time;
            var lslTimestamp = Lsl.local_clock();

            var streamClose = new StreamClose
            {
                StreamInfo = new Sample.LSL.StreamInfo
                {
                    PlumeRawTimestamp = plumeTimestamp,
                    LslPlumeOffset = _lslPlumeOffset,
                    LslStreamId = inlet.StreamId.ToString(),
                    LslTimestamp = lslTimestamp,
                    LslClockOffset = 0
                }
            };

            ctx.CurrentRecord.RecordTimestampedManagedSample(streamClose, plumeTimestamp);
        }


        private void RecordStreamSampleChunk(BufferedInlet inlet, ICollection<ICollection> samples,
            ICollection<double> timestamps, RecorderContext ctx)
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

                var plumeRawTimestamp = ctx.CurrentRecord.Time;
                var lslTimestamp = timestamps.ElementAt(sampleIdx);
                var lslClockOffset = inlet.TimeCorrection();
                var correctedLslTimestamp = lslTimestamp + lslClockOffset;
                var correctedPlumeTimestamp = (long)(correctedLslTimestamp * 1_000_000_000 + _lslPlumeOffset);

                if (correctedPlumeTimestamp < 0)
                    return;

                streamSample.StreamInfo = new Sample.LSL.StreamInfo
                {
                    PlumeRawTimestamp = plumeRawTimestamp,
                    LslPlumeOffset = _lslPlumeOffset,
                    LslStreamId = inlet.StreamId.ToString(),
                    LslTimestamp = lslTimestamp,
                    LslClockOffset = lslClockOffset
                };

                ctx.CurrentRecord.RecordTimestampedManagedSample(streamSample, (ulong)correctedPlumeTimestamp);
            }
        }
    }
}