using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using PLUME.Sample;
using UnityEngine;
using CompressionLevel = System.IO.Compression.CompressionLevel;
using pb = Google.Protobuf;

namespace PLUME
{
    public class RecordWriter : IDisposable
    {
        private readonly RecordMetadata _recordMetadata;
        private readonly RecorderClock _recorderClock;
        private readonly SamplePoolManager _samplePoolManager;

        private readonly int _bufferSize;
        private readonly string _path;
        private readonly string _metadataPath;

        private readonly CompressionLevel _compressionLevel;
        
        private readonly UnpackedSampleSortedList _unpackedSamples;

        private bool _closed;

        private bool _stopThread;
        private readonly Thread _thread;

        /*
         * Delay before writing samples to disk in nanoseconds. This allows samples arriving late to still be sorted in order
         * to keep the record file sequential (ordered by ascending timestamp).
         */
        public const ulong SampleWriteDelay = 5_000_000_000;

        public RecordWriter(RecorderClock recorderClock, SamplePoolManager samplePoolManager, string path,
            CompressionLevel compressionLevel, RecordMetadata recordMetadata, int bufferSize = 4096)
        {
            _recordMetadata = recordMetadata;
            _recorderClock = recorderClock;
            _samplePoolManager = samplePoolManager;

            _path = path;
            _metadataPath = path + ".meta";
            _bufferSize = bufferSize;
            _compressionLevel = compressionLevel;

            _unpackedSamples = new UnpackedSampleSortedList();

            _thread = new Thread(Run);
            _thread.Start();
        }

        private void Run()
        {
            ulong writtenSampleCount = 0;
            ulong writtenSampleMaxTimestamp = 0;

            using var metadataStream = File.Create(_metadataPath, _bufferSize, FileOptions.RandomAccess);
            using var samplesStream = new GZipStream(File.Create(_path, _bufferSize), _compressionLevel);

            _recordMetadata.Sequential = true;
            UpdateMetadata(_recordMetadata, metadataStream);

            do
            {
                if (_stopThread)
                {
                    Debug.Log($"{_unpackedSamples.Count} samples left to write.");
                }

                while (true)
                {
                    var peekEntry = _unpackedSamples.Peek();

                    if (peekEntry == null)
                        break;

                    var peekEntryHeader = peekEntry.Header;
                    
                    var shouldWriteSample = _stopThread || peekEntryHeader == null ||
                                            _recorderClock.GetTimeInNanoseconds() > SampleWriteDelay
                                            && peekEntryHeader.Time <=
                                            _recorderClock.GetTimeInNanoseconds() - SampleWriteDelay;

                    if (!shouldWriteSample)
                        break;
                    
                    _unpackedSamples.TryTake(out var entry);

                    var unpackedSample = entry;
                    PackedSample packedSample;
                    
                    if (unpackedSample.Header != null)
                    {
                        packedSample = _samplePoolManager.GetPackedSampleStamped();
                        packedSample.Header.Time = unpackedSample.Header.Time;
                        packedSample.Header.Seq = unpackedSample.Header.Seq;
                        packedSample.Payload = Any.Pack(unpackedSample.Payload);
                        packedSample.WriteDelimitedTo(samplesStream);
                        _samplePoolManager.ReleaseSamplePayload(unpackedSample.Payload);
                        _samplePoolManager.ReleaseUnpackedSampleStamped(unpackedSample);
                    }
                    else
                    {
                        packedSample = _samplePoolManager.GetPackedSample();
                        packedSample.Payload = Any.Pack(unpackedSample.Payload);
                        packedSample.WriteDelimitedTo(samplesStream);
                        _samplePoolManager.ReleaseSamplePayload(unpackedSample.Payload);
                        _samplePoolManager.ReleaseUnpackedSample(unpackedSample);
                    }

                    writtenSampleCount++;

                    if (packedSample.Header != null)
                    {
                        if (packedSample.Header.Time < writtenSampleMaxTimestamp && _recordMetadata.Sequential)
                        {
                            Debug.Log(packedSample.Header.Time + " " + writtenSampleMaxTimestamp);
                            Debug.LogWarning("Record is no longer sequential.");
                            _recordMetadata.Sequential = false;
                        }

                        writtenSampleMaxTimestamp = Math.Max(writtenSampleMaxTimestamp, packedSample.Header.Time);
                        _samplePoolManager.ReleasePackedSampleStamped(packedSample);
                    }
                    else
                    {
                        _samplePoolManager.ReleasePackedSample(packedSample);
                    }
                }
                
                _recordMetadata.SamplesCount = writtenSampleCount;
                _recordMetadata.Duration = Math.Max(writtenSampleMaxTimestamp, _recorderClock.GetTimeInNanoseconds());
                UpdateMetadata(_recordMetadata, metadataStream);
            } while (!_stopThread || _unpackedSamples.Count > 0);

            _recordMetadata.SamplesCount = writtenSampleCount;
            _recordMetadata.Duration = Math.Max(writtenSampleMaxTimestamp, _recorderClock.GetTimeInNanoseconds());
            UpdateMetadata(_recordMetadata, metadataStream);
        }

        private static void UpdateMetadata(RecordMetadata recordMetadata, Stream headerStream)
        {
            headerStream.Seek(0, SeekOrigin.Begin);
            headerStream.SetLength(0);
            recordMetadata.WriteDelimitedTo(headerStream);
        }

        public void Write(UnpackedSample unpackedSample)
        {
            if (_closed)
                throw new Exception("Can't record samples when the record writer is closed.");

            _unpackedSamples.Add(unpackedSample);
        }

        public void Close()
        {
            if (_closed)
                return;
            _closed = true;

            _stopThread = true;
            Debug.Log($"Waiting for the writing thread to write {_unpackedSamples.Count} samples to disk.");
            _thread.Join();

            var fileInfo = new FileInfo(_path);
            Debug.LogFormat("Finished writing record at {0} ({1})", _path, GetSizeString(fileInfo.Length));
        }

        public void Dispose()
        {
            Close();
        }

        private static string GetSizeString(long length)
        {
            const long kb = 1024L;
            const long mb = kb * 1024L;
            const long gb = mb * 1024L;
            const long tb = gb * 1024L;

            double size = length;
            var suffix = "B";

            switch (length)
            {
                case >= tb:
                    size = Math.Round((double)length / tb, 2);
                    suffix = "TB";
                    break;
                case >= gb:
                    size = Math.Round((double)length / gb, 2);
                    suffix = "GB";
                    break;
                case >= mb:
                    size = Math.Round((double)length / mb, 2);
                    suffix = "MB";
                    break;
                case >= kb:
                    size = Math.Round((double)length / kb, 2);
                    suffix = "KB";
                    break;
            }

            return $"{size}{suffix}";
        }
    }
}