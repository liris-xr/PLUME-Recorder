using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Google.Protobuf;
using PLUME.Base.Settings;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module;
using PLUME.Sample.LSL;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Collections;
using UnityEngine.Pool;
using UnityEngine.Profiling;
using UnityEngine.Scripting;
using Logger = PLUME.Core.Logger;

namespace PLUME.Base.Module.LSL
{
    [Preserve]
    public class LslRecorderModule : RecorderModule
    {
        private LslRecorderModuleSettings _settings;

        private ObjectPool<StreamSample> _streamSamplePool;
        private ObjectPool<StreamSample.Types.RepeatedInt8> _repeatedInt8Pool;
        private ObjectPool<StreamSample.Types.RepeatedInt16> _repeatedInt16Pool;
        private ObjectPool<StreamSample.Types.RepeatedInt32> _repeatedInt32Pool;
        private ObjectPool<StreamSample.Types.RepeatedInt64> _repeatedInt64Pool;
        private ObjectPool<StreamSample.Types.RepeatedFloat> _repeatedFloatPool;
        private ObjectPool<StreamSample.Types.RepeatedDouble> _repeatedDoublePool;
        private ObjectPool<StreamSample.Types.RepeatedString> _repeatedStringPool;

        private SampleTypeUrl _streamSampleType;

        private readonly List<BufferedInlet> _resolvedStreams = new();
        private Thread _streamResolverThread;
        private Thread _streamRecorderThread;

        private const int SerializationBufferSize = 1024;
        private MemoryStream _tmpMemoryStream;
        private CodedOutputStream _tmpCodedOutputStream;

        // LSL local clock timestamp at the start of the recording
        private double _lslRecordingStartTime;

        protected override void OnCreate(RecorderContext ctx)
        {
            base.OnCreate(ctx);
            _settings = ctx.SettingsProvider.GetOrCreate<LslRecorderModuleSettings>();

            _streamSamplePool = new ObjectPool<StreamSample>(() => new StreamSample());
            _repeatedInt8Pool =
                new ObjectPool<StreamSample.Types.RepeatedInt8>(() => new StreamSample.Types.RepeatedInt8(),
                    s => s.Value.Clear());
            _repeatedInt16Pool =
                new ObjectPool<StreamSample.Types.RepeatedInt16>(() => new StreamSample.Types.RepeatedInt16(),
                    s => s.Value.Clear());
            _repeatedInt32Pool =
                new ObjectPool<StreamSample.Types.RepeatedInt32>(() => new StreamSample.Types.RepeatedInt32(),
                    s => s.Value.Clear());
            _repeatedInt64Pool =
                new ObjectPool<StreamSample.Types.RepeatedInt64>(() => new StreamSample.Types.RepeatedInt64(),
                    s => s.Value.Clear());
            _repeatedFloatPool =
                new ObjectPool<StreamSample.Types.RepeatedFloat>(() => new StreamSample.Types.RepeatedFloat(),
                    s => s.Value.Clear());
            _repeatedDoublePool =
                new ObjectPool<StreamSample.Types.RepeatedDouble>(() => new StreamSample.Types.RepeatedDouble(),
                    s => s.Value.Clear());
            _repeatedStringPool =
                new ObjectPool<StreamSample.Types.RepeatedString>(() => new StreamSample.Types.RepeatedString(),
                    s => s.Value.Clear());

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
            _lslRecordingStartTime = Lsl.local_clock();

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

                            if (nPulledSamples > 0)
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

                    var bufferedInlet = CreateBufferedInlet(channelFormat, streamInfo, nextLslStreamId++);

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

        private static BufferedInlet CreateBufferedInlet(channel_format_t channelFormat, StreamInfo streamInfo, uint id)
        {
            BufferedInlet bufferedInlet = channelFormat switch
            {
                channel_format_t.cf_float32 => new BufferedInlet<float>(streamInfo, id),
                channel_format_t.cf_double64 => new BufferedInlet<double>(streamInfo, id),
                channel_format_t.cf_string => new BufferedInlet<string>(streamInfo, id),
                channel_format_t.cf_int32 => new BufferedInlet<int>(streamInfo, id),
                channel_format_t.cf_int16 => new BufferedInlet<short>(streamInfo, id),
                channel_format_t.cf_int8 => new BufferedInlet<sbyte>(streamInfo, id),
                channel_format_t.cf_int64 => new BufferedInlet<long>(streamInfo, id),
                channel_format_t.cf_undefined => throw new Exception(
                    $"Unsupported channel format {channelFormat}"),
                _ => throw new ArgumentOutOfRangeException()
            };
            return bufferedInlet;
        }

        private void RecordOpenStream(BufferedInlet inlet, RecorderContext ctx)
        {
            // LSL timestamp corrected using the NTP protocol. Precision of these estimates should be below 1 ms.
            // Change the time referential so t=0 corresponds to the start of the recording.
            var lslCorrectedTimestamp = inlet.CreatedAt + inlet.TimeCorrection() - _lslRecordingStartTime;

            // If the stream was opened before we started recording, assume it was opened at t=0.
            if (lslCorrectedTimestamp < 0)
                lslCorrectedTimestamp = 0;

            // Convert the timestamp to nanoseconds
            var correctedTimestamp = (ulong)(lslCorrectedTimestamp * 1_000_000_000);

            var streamOpen = new StreamOpen { StreamId = inlet.StreamId, XmlHeader = inlet.InfoXml };
            ctx.CurrentRecord.RecordTimestampedManagedSample(streamOpen, correctedTimestamp);
        }

        private void RecordCloseStream(BufferedInlet inlet, RecorderContext ctx)
        {
            // We can't query the inlet time correction when closed.
            // So we assume that the stream is closed at the current recorder timestamp.
            var streamClose = new StreamClose { StreamId = inlet.StreamId };
            ctx.CurrentRecord.RecordTimestampedManagedSample(streamClose, ctx.CurrentRecord.Time);
        }

        // TODO: refactor this method
        private void RecordStreamSampleChunk(BufferedInlet inlet, int nSamples, Array channelValues,
            double[] sampleTimestamps,
            RecorderContext ctx)
        {
            for (var i = 0; i < nSamples; i++)
            {
                var timeCorrection = inlet.TimeCorrection();
                var sampleTimestamp = sampleTimestamps[i];

                // LSL timestamp corrected using the NTP protocol. Precision of these estimates should be below 1 ms.
                // Change the time referential so t=0 corresponds to the start of the recording.
                var lslCorrectedTimestamp = sampleTimestamp + timeCorrection;

                // If the sample was emitted before starting the record, we discard it.
                if (lslCorrectedTimestamp - _lslRecordingStartTime < 0)
                    continue;

                // Convert the timestamp to nanoseconds
                var correctedTimestamp = (ulong)((lslCorrectedTimestamp - _lslRecordingStartTime) * 1_000_000_000);

                var streamSample = _streamSamplePool.Get();
                streamSample.StreamId = inlet.StreamId;
                
                switch (inlet.ChannelFormat)
                {
                    case channel_format_t.cf_float32:
                        streamSample.FloatValues = _repeatedFloatPool.Get();
                        for (var channelIdx = 0; channelIdx < inlet.ChannelCount; channelIdx++)
                        {
                            streamSample.FloatValues.Value.Add(
                                ((float[])channelValues)[i * inlet.ChannelCount + channelIdx]);
                        }
                        break;
                    case channel_format_t.cf_double64:
                        streamSample.DoubleValues = _repeatedDoublePool.Get();
                        for (var channelIdx = 0; channelIdx < inlet.ChannelCount; channelIdx++)
                        {
                            streamSample.DoubleValues.Value.Add(
                                ((double[])channelValues)[i * inlet.ChannelCount + channelIdx]);
                        }
                        break;
                    case channel_format_t.cf_string:
                        streamSample.StringValues = _repeatedStringPool.Get();
                        for (var channelIdx = 0; channelIdx < inlet.ChannelCount; channelIdx++)
                        {
                            streamSample.StringValues.Value.Add(
                                ((string[])channelValues)[i * inlet.ChannelCount + channelIdx]);
                        }
                        break;
                    case channel_format_t.cf_int8:
                        streamSample.Int8Values = _repeatedInt8Pool.Get();
                        for (var channelIdx = 0; channelIdx < inlet.ChannelCount; channelIdx++)
                        {
                            streamSample.Int8Values.Value.Add(
                                ((sbyte[])channelValues)[i * inlet.ChannelCount + channelIdx]);
                        }
                        break;
                    case channel_format_t.cf_int16:
                        streamSample.Int16Values = _repeatedInt16Pool.Get();
                        for (var channelIdx = 0; channelIdx < inlet.ChannelCount; channelIdx++)
                        {
                            streamSample.Int16Values.Value.Add(
                                ((short[])channelValues)[i * inlet.ChannelCount + channelIdx]);
                        }
                        break;
                    case channel_format_t.cf_int32:
                        streamSample.Int32Values = _repeatedInt32Pool.Get();
                        for (var channelIdx = 0; channelIdx < inlet.ChannelCount; channelIdx++)
                        {
                            streamSample.Int32Values.Value.Add(
                                ((int[])channelValues)[i * inlet.ChannelCount + channelIdx]);
                        }
                        break;
                    case channel_format_t.cf_int64:
                        streamSample.Int64Values = _repeatedInt64Pool.Get();
                        for (var channelIdx = 0; channelIdx < inlet.ChannelCount; channelIdx++)
                        {
                            streamSample.Int64Values.Value.Add(
                                ((long[])channelValues)[i * inlet.ChannelCount + channelIdx]);
                        }
                        break;
                    case channel_format_t.cf_undefined:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                if (streamSample.CalculateSize() < SerializationBufferSize)
                {
                    _tmpMemoryStream.Position = 0;
                    _tmpMemoryStream.SetLength(0);
                    streamSample.WriteTo(_tmpCodedOutputStream);
                    _tmpCodedOutputStream.Flush();
                    var bytes = _tmpMemoryStream.GetBuffer().AsSpan(0, (int)_tmpMemoryStream.Length);
                    ctx.CurrentRecord.RecordTimestampedSample(bytes, _streamSampleType, correctedTimestamp);
                }
                else
                {
                    var bytes = streamSample.ToByteArray();
                    ctx.CurrentRecord.RecordTimestampedSample(bytes, _streamSampleType, correctedTimestamp);
                }

                switch (inlet.ChannelFormat)
                {
                    case channel_format_t.cf_float32:
                        _repeatedFloatPool.Release(streamSample.FloatValues);
                        break;
                    case channel_format_t.cf_double64:
                        _repeatedDoublePool.Release(streamSample.DoubleValues);
                        break;
                    case channel_format_t.cf_string:
                        _repeatedStringPool.Release(streamSample.StringValues);
                        break;
                    case channel_format_t.cf_int8:
                        _repeatedInt8Pool.Release(streamSample.Int8Values);
                        break;
                    case channel_format_t.cf_int16:
                        _repeatedInt16Pool.Release(streamSample.Int16Values);
                        break;
                    case channel_format_t.cf_int32:
                        _repeatedInt32Pool.Release(streamSample.Int32Values);
                        break;
                    case channel_format_t.cf_int64:
                        _repeatedInt64Pool.Release(streamSample.Int64Values);
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