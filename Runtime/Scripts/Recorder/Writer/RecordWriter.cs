﻿using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using Google.Protobuf;
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

        private CompressionLevel _compressionLevel;

        private readonly OrderedPackedSampleList _orderedPackedSamples;
        private readonly SamplePacker _samplePacker;

        private bool _closed;

        private bool _stopThread;
        private readonly Thread _thread;

        /*
         * Delay before writing samples to disk in nanoseconds. This allows samples arriving late to still be sorted in order
         * to keep the record file sequential (ordered by ascending timestamp).
         */
        private const ulong SampleWriteDelay = 5_000_000_000;

        public RecordWriter(RecorderClock recorderClock, SamplePoolManager samplePoolManager, string path, CompressionLevel compressionLevel, RecordMetadata recordMetadata, int bufferSize = 4096)
        {
            _recordMetadata = recordMetadata;
            _recorderClock = recorderClock;
            _samplePoolManager = samplePoolManager;
            
            _path = path;
            _bufferSize = bufferSize;
            _compressionLevel = compressionLevel;

            _orderedPackedSamples = new OrderedPackedSampleList();
            _samplePacker = new SamplePacker(samplePoolManager, _orderedPackedSamples);
            
            _thread = new Thread(Run);
            _thread.Start();
        }

        private void Run()
        {
            ulong writtenSampleCount = 0;
            ulong writtenSampleMaxTimestamp = 0;
            
            using var stream = File.Create(_path, _bufferSize, FileOptions.RandomAccess);
            using var archive = new ZipArchive(stream, ZipArchiveMode.Update);
            using var metadata = archive.CreateEntry("metadata", _compressionLevel).Open();
            using var samples = archive.CreateEntry("samples", _compressionLevel).Open();
            
            _recordMetadata.Sequential = true;
            UpdateMetadata(_recordMetadata, metadata);
            
            do
            {
                while (!_orderedPackedSamples.IsEmpty())
                {
                    var shouldWriteSample = _stopThread ||
                                            (_recorderClock.GetTimeInNanoseconds() > SampleWriteDelay
                                             && _orderedPackedSamples.Peek().Header.Time >=
                                             _recorderClock.GetTimeInNanoseconds() - SampleWriteDelay);
            
                    if (!shouldWriteSample)
                        break;
            
                    _orderedPackedSamples.TryTake(out var sampleToWrite);
                    
                    if (sampleToWrite.Header.Time < writtenSampleMaxTimestamp && _recordMetadata.Sequential)
                    {
                        _recordMetadata.Sequential = false;
                    }
                    sampleToWrite.WriteDelimitedTo(samples);
            
                    writtenSampleCount++;
                    writtenSampleMaxTimestamp = Math.Max(writtenSampleMaxTimestamp, sampleToWrite.Header.Time);
                    _samplePoolManager.ReleasePackedSample(sampleToWrite);
                    
                    _recordMetadata.SamplesCount = writtenSampleCount;
                    _recordMetadata.Duration = Math.Max(writtenSampleMaxTimestamp, _recorderClock.GetTimeInNanoseconds());
                    UpdateMetadata(_recordMetadata, metadata);
                }
            } while (!_stopThread || _orderedPackedSamples.Count > 0);
            
            _recordMetadata.SamplesCount = writtenSampleCount;
            _recordMetadata.Duration = Math.Max(writtenSampleMaxTimestamp, _recorderClock.GetTimeInNanoseconds());
            UpdateMetadata(_recordMetadata, metadata);
            
            metadata.Flush();
            samples.Flush();
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

            _samplePacker.Enqueue(unpackedSample);
        }

        public void Close()
        {
            if (_closed)
                return;
            _closed = true;

            _samplePacker.Stop();
            _samplePacker.Join();

            _stopThread = true;
            Debug.Log($"Waiting for the writing thread to write {_orderedPackedSamples.Count} samples to disk.");
            _thread.Join();

            var fileInfo = new FileInfo(_path);
            Debug.Log("Final file size: " + GetSizeString(fileInfo.Length));
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