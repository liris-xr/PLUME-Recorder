using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using Google.Protobuf.WellKnownTypes;
using PLUME.Sample;
using UnityEngine;
using UnityEngine.Profiling;

namespace PLUME
{
    public class ThreadedRecordWriter : IDisposable
    {
        private readonly RecordWriter _writer;

        private readonly ConcurrentQueue<UnpackedSample> _unpackedSamples;
        private readonly Thread _thread;
        private bool _stopThread;

        private bool _closed;

        private readonly SamplePoolManager _samplePoolManager;

        public ThreadedRecordWriter(SamplePoolManager samplePoolManager, string filepath, string recordIdentifier, bool leaveOpen = false)
        {
            _samplePoolManager = samplePoolManager;
            _writer = new RecordWriter(filepath, recordIdentifier, leaveOpen);
            _unpackedSamples = new ConcurrentQueue<UnpackedSample>();
            _thread = new Thread(PackSamples);
            _thread.Start();
        }

        public void Write(UnpackedSample unpackedSample)
        {
            if (_stopThread)
            {
                throw new Exception("The packer thread is stopping. Adding new samples is not allowed.");
            }

            _unpackedSamples.Enqueue(unpackedSample);
        }

        private void PackSamples()
        {
            while (!_stopThread || _unpackedSamples.Count > 0)
            {
                try
                {
                    if (_unpackedSamples.TryDequeue(out var unpackedSample))
                    {
                        var packedPayload = Any.Pack(unpackedSample.Payload);
                        var packedSample = new PackedSample {Header = unpackedSample.Header, Payload = packedPayload};
                        _writer.WriteSample(packedSample);
                        
                        _samplePoolManager.ReleaseSamplePayload(unpackedSample.Payload);
                        _samplePoolManager.ReleaseUnpackedSample(unpackedSample);
                    }
                }
                catch (ThreadInterruptedException)
                {
                    // ignored
                }
            }
        }

        public void Close()
        {
            if (_closed)
                return;

            Debug.Log($"Waiting for the {_unpackedSamples.Count} samples left to be packed and saved.");

            // Stop thread and wait for the queue to be empty
            _stopThread = true;
            _thread.Join();

            _writer.Close();
            _closed = true;
        }

        public void Dispose()
        {
            Close();
        }
    }
}