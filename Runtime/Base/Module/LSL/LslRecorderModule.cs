using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Google.Protobuf;
using PLUME.Base.Settings;
using PLUME.Core;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module;
using PLUME.Sample.LSL;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Collections;
using UnityEngine.Pool;
using UnityEngine.Profiling;
using UnityEngine.Scripting;

namespace PLUME.Base.Module.LSL
{
    [Preserve]
    public class LslRecorderModule : RecorderModule
    {
        private LslRecorderModuleSettings _settings;

        private ObjectPool<StreamSample> _streamSamplePool;
        private ObjectPool<Sample.LSL.StreamInfo> _streamInfoPool;
        private ObjectPool<RepeatedInt8> _repeatedInt8Pool;
        private ObjectPool<RepeatedInt16> _repeatedInt16Pool;
        private ObjectPool<RepeatedInt32> _repeatedInt32Pool;
        private ObjectPool<RepeatedInt64> _repeatedInt64Pool;
        private ObjectPool<RepeatedFloat> _repeatedFloatPool;
        private ObjectPool<RepeatedDouble> _repeatedDoublePool;
        private ObjectPool<RepeatedString> _repeatedStringPool;

        private SampleTypeUrl _streamSampleType;
        
        private readonly List<BufferedInlet> _resolvedStreams = new();
        private Thread _streamResolverThread;
        private Thread _streamRecorderThread;

        private const int SerializationBufferSize = 1024;
        private MemoryStream _tmpMemoryStream;
        private CodedOutputStream _tmpCodedOutputStream;

        // Offset (in nanoseconds) between the LSL clock and the recorder clock
        private long _lslPlumeOffset;

        protected override void OnCreate(RecorderContext ctx)
        {
            base.OnCreate(ctx);
            _settings = ctx.SettingsProvider.GetOrCreate<LslRecorderModuleSettings>();

            _streamSamplePool = new ObjectPool<StreamSample>(() => new StreamSample());
            _streamInfoPool = new ObjectPool<Sample.LSL.StreamInfo>(() => new Sample.LSL.StreamInfo());
            _repeatedInt8Pool = new ObjectPool<RepeatedInt8>(() => new RepeatedInt8(), s => s.Value.Clear());
            _repeatedInt16Pool = new ObjectPool<RepeatedInt16>(() => new RepeatedInt16(), s => s.Value.Clear());
            _repeatedInt32Pool = new ObjectPool<RepeatedInt32>(() => new RepeatedInt32(), s => s.Value.Clear());
            _repeatedInt64Pool = new ObjectPool<RepeatedInt64>(() => new RepeatedInt64(), s => s.Value.Clear());
            _repeatedFloatPool = new ObjectPool<RepeatedFloat>(() => new RepeatedFloat(), s => s.Value.Clear());
            _repeatedDoublePool = new ObjectPool<RepeatedDouble>(() => new RepeatedDouble(), s => s.Value.Clear());
            _repeatedStringPool = new ObjectPool<RepeatedString>(() => new RepeatedString(), s => s.Value.Clear());
            
            _streamSampleType = SampleTypeUrl.Alloc(StreamSample.Descriptor, Allocator.Persistent);
            _tmpMemoryStream = new MemoryStream(SerializationBufferSize);
            _tmpCodedOutputStream = new CodedOutputStream(_tmpMemoryStream);
        }

        protected override void OnDestroy(RecorderContext ctx)
        {
            base.OnDestroy(ctx);
            _streamSampleType.Dispose();
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

            lock (_resolvedStreams)
            {
                foreach (var resolvedStream in _resolvedStreams)
                {
                    resolvedStream.Close();
                }

                _resolvedStreams.Clear();
            }

            _streamRecorderThread.Join();

            lock (_resolvedStreams)
            {
                _resolvedStreams.Clear();
            }
        }

        private void LslStreamRecorderLoop(RecorderContext ctx)
        {
            Profiler.BeginThreadProfiling("PLUME", "LslRecorderModule.StreamRecorder");

            while (ctx.IsRecording)
            {
                lock (_resolvedStreams)
                {
                    foreach (var stream in _resolvedStreams)
                    {
                        int nPulledSamples;

                        do
                        {
                            nPulledSamples = stream.PullChunk();

                            if (nPulledSamples != 0)
                            {
                                RecordStreamSampleChunk(stream, nPulledSamples, stream.GetDataBuffer(),
                                    stream.GetTimestampBuffer(), ctx);
                            }
                        } while (nPulledSamples != 0);
                    }
                }
            }

            Profiler.EndThreadProfiling();
        }

        private void LslStreamResolverLoop(RecorderContext ctx)
        {
            Profiler.BeginThreadProfiling("PLUME", "LslRecorderModule.StreamResolver");

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
                        lastKnownUIDs.Add(resolvedStream.Uid);
                        lastKnownSourceIds.Add(resolvedStream.SourceId);
                        forgottenStreamsByUid.Add(resolvedStream.Uid, resolvedStream);
                    }
                }

                foreach (var streamInfo in streamResolver.results())
                {
                    var streamSourceId = streamInfo.source_id();
                    var streamUid = streamInfo.uid();
                    var channelFormat = streamInfo.channel_format();
                    var streamName = streamInfo.name();

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

                    Logger.Log($"Resolved new stream {streamName} with UID {streamUid} and source ID {streamSourceId}");
                    RecordOpenStream(bufferedInlet, ctx);

                    lock (_resolvedStreams)
                    {
                        _resolvedStreams.Add(bufferedInlet);
                    }
                }

                foreach (var (uid, forgottenStream) in forgottenStreamsByUid)
                {
                    var sourceId = forgottenStream.SourceId;
                    var name = forgottenStream.Name;

                    lock (_resolvedStreams)
                    {
                        if (!string.IsNullOrEmpty(sourceId) &&
                            _resolvedStreams.FirstOrDefault(s => s.Uid != uid && s.SourceId == sourceId) != null)
                        {
                            continue;
                        }

                        lock (_resolvedStreams)
                        {
                            _resolvedStreams.Remove(forgottenStream);
                        }
                    }

                    Logger.Log($"Lost stream {name} with UID {uid} and source ID {sourceId}.");
                    RecordCloseStream(forgottenStream, ctx);
                }

                if (ctx.IsRecording)
                    Thread.Sleep(_settings.ResolveInterval);
            }

            Profiler.EndThreadProfiling();
        }

        private void RecordOpenStream(BufferedInlet inlet, RecorderContext ctx)
        {
            var plumeRawTimestamp = ctx.CurrentRecord.Time;
            var lslTimestamp = inlet.CreatedAt;
            var lslClockOffset = inlet.TimeCorrection();
            var correctedLslTimestamp = lslTimestamp + lslClockOffset;
            var correctedPlumeTimestamp = (long)(correctedLslTimestamp * 1_000_000_000 + _lslPlumeOffset);

            if (correctedPlumeTimestamp < 0) return;

            var streamInfo = _streamInfoPool.Get();
            streamInfo.PlumeRawTimestamp = plumeRawTimestamp;
            streamInfo.LslPlumeOffset = _lslPlumeOffset;
            streamInfo.LslStreamId = inlet.StreamId;
            streamInfo.LslTimestamp = lslTimestamp;
            streamInfo.LslClockOffset = lslClockOffset;
            var streamOpen = new StreamOpen { StreamInfo = streamInfo, XmlHeader = inlet.InfoXml };

            ctx.CurrentRecord.RecordTimestampedManagedSample(streamOpen, (ulong)correctedPlumeTimestamp);
            _streamInfoPool.Release(streamInfo);
        }

        private void RecordCloseStream(BufferedInlet inlet, RecorderContext ctx)
        {
            // We can't query the inlet time correction when closed, we directly use the recorder timestamp.
            var plumeTimestamp = ctx.CurrentRecord.Time;
            var lslTimestamp = Lsl.local_clock();

            var streamInfo = _streamInfoPool.Get();
            streamInfo.PlumeRawTimestamp = plumeTimestamp;
            streamInfo.LslPlumeOffset = _lslPlumeOffset;
            streamInfo.LslStreamId = inlet.StreamId;
            streamInfo.LslTimestamp = lslTimestamp;
            streamInfo.LslClockOffset = 0;

            var streamClose = new StreamClose { StreamInfo = streamInfo };
            ctx.CurrentRecord.RecordTimestampedManagedSample(streamClose, plumeTimestamp);
            _streamInfoPool.Release(streamInfo);
        }
        
        private void RecordStreamSampleChunk(BufferedInlet inlet, int nSamples, Array channelValues, double[] sampleTimestamps,
            RecorderContext ctx)
        {
            for (var i = 0; i < nSamples; i++)
            {
                var streamSample = _streamSamplePool.Get();
                
                switch (inlet.ChannelFormat)
                {
                    case channel_format_t.cf_float32:
                        streamSample.FloatValue = _repeatedFloatPool.Get();
                        for(var channelIdx = 0; channelIdx < inlet.ChannelCount; channelIdx++)
                        {
                            streamSample.FloatValue.Value.Add(((float[])channelValues)[i * inlet.ChannelCount + channelIdx]);
                        }
                        break;
                    case channel_format_t.cf_double64:
                        streamSample.DoubleValue = _repeatedDoublePool.Get();
                        for(var channelIdx = 0; channelIdx < inlet.ChannelCount; channelIdx++)
                        {
                            streamSample.DoubleValue.Value.Add(((double[])channelValues)[i * inlet.ChannelCount + channelIdx]);
                        }
                        break;
                    case channel_format_t.cf_string:
                        streamSample.StringValue = _repeatedStringPool.Get();
                        for(var channelIdx = 0; channelIdx < inlet.ChannelCount; channelIdx++)
                        {
                            streamSample.StringValue.Value.Add(((string[])channelValues)[i * inlet.ChannelCount + channelIdx]);
                        }
                        break;
                    case channel_format_t.cf_int8:
                        streamSample.Int8Value = _repeatedInt8Pool.Get();
                        for(var channelIdx = 0; channelIdx < inlet.ChannelCount; channelIdx++)
                        {
                            streamSample.Int8Value.Value.Add(((sbyte[])channelValues)[i * inlet.ChannelCount + channelIdx]);
                        }
                        break;
                    case channel_format_t.cf_int16:
                        streamSample.Int16Value = _repeatedInt16Pool.Get();
                        for(var channelIdx = 0; channelIdx < inlet.ChannelCount; channelIdx++)
                        {
                            streamSample.Int16Value.Value.Add(((short[])channelValues)[i * inlet.ChannelCount + channelIdx]);
                        }
                        break;
                    case channel_format_t.cf_int32:
                        streamSample.Int32Value = _repeatedInt32Pool.Get();
                        for(var channelIdx = 0; channelIdx < inlet.ChannelCount; channelIdx++)
                        {
                            streamSample.Int32Value.Value.Add(((int[])channelValues)[i * inlet.ChannelCount + channelIdx]);
                        }
                        break;
                    case channel_format_t.cf_int64:
                        streamSample.Int64Value = _repeatedInt64Pool.Get();
                        for(var channelIdx = 0; channelIdx < inlet.ChannelCount; channelIdx++)
                        {
                            streamSample.Int64Value.Value.Add(((long[])channelValues)[i * inlet.ChannelCount + channelIdx]);
                        }
                        break;
                    case channel_format_t.cf_undefined:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var plumeRawTimestamp = ctx.CurrentRecord.Time;
                var lslTimestamp = sampleTimestamps[i];
                var lslClockOffset = inlet.TimeCorrection();
                var correctedLslTimestamp = lslTimestamp + lslClockOffset;
                var correctedPlumeTimestamp = (long)(correctedLslTimestamp * 1_000_000_000 + _lslPlumeOffset);

                if (correctedPlumeTimestamp < 0)
                    return;

                streamSample.StreamInfo = _streamInfoPool.Get();
                streamSample.StreamInfo.PlumeRawTimestamp = plumeRawTimestamp;
                streamSample.StreamInfo.LslPlumeOffset = _lslPlumeOffset;
                streamSample.StreamInfo.LslStreamId = inlet.StreamId;
                streamSample.StreamInfo.LslTimestamp = lslTimestamp;
                streamSample.StreamInfo.LslClockOffset = lslClockOffset;

                if (streamSample.CalculateSize() < SerializationBufferSize)
                {
                    _tmpMemoryStream.Position = 0;
                    _tmpMemoryStream.SetLength(0);
                    streamSample.WriteTo(_tmpCodedOutputStream);
                    var bytes = _tmpMemoryStream.GetBuffer().AsSpan(0, (int)_tmpMemoryStream.Length);
                    ctx.CurrentRecord.RecordTimestampedSample(bytes, _streamSampleType, (ulong)correctedPlumeTimestamp);
                }
                else
                {
                    var bytes = streamSample.ToByteArray();
                    ctx.CurrentRecord.RecordTimestampedSample(bytes, _streamSampleType, (ulong)correctedPlumeTimestamp);
                }

                _streamInfoPool.Release(streamSample.StreamInfo);

                switch (inlet.ChannelFormat)
                {
                    case channel_format_t.cf_float32:
                        _repeatedFloatPool.Release(streamSample.FloatValue);
                        break;
                    case channel_format_t.cf_double64:
                        _repeatedDoublePool.Release(streamSample.DoubleValue);
                        break;
                    case channel_format_t.cf_string:
                        _repeatedStringPool.Release(streamSample.StringValue);
                        break;
                    case channel_format_t.cf_int8:
                        _repeatedInt8Pool.Release(streamSample.Int8Value);
                        break;
                    case channel_format_t.cf_int16:
                        _repeatedInt16Pool.Release(streamSample.Int16Value);
                        break;
                    case channel_format_t.cf_int32:
                        _repeatedInt32Pool.Release(streamSample.Int32Value);
                        break;
                    case channel_format_t.cf_int64:
                        _repeatedInt64Pool.Release(streamSample.Int64Value);
                        break;
                    case channel_format_t.cf_undefined:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                _streamSamplePool.Release(streamSample);
            }
        }
    }
}