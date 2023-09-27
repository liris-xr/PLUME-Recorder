using System.Collections.Concurrent;
using System.Threading;
using Google.Protobuf.WellKnownTypes;
using PLUME.Sample;

namespace PLUME
{
    public class SamplePacker
    {
        private readonly SamplePoolManager _samplePoolManager;

        private readonly Thread[] _packingThreads;
        private readonly OrderedSampleList _samples;
        
        // TODO: prioritize packing by timestamp
        private readonly ConcurrentQueue<UnpackedSample> _unpackedSamples = new ();

        private bool _shouldStop;

        public SamplePacker(SamplePoolManager samplePoolManager, OrderedSampleList samples, int nPackerThreads = 4)
        {
            _samplePoolManager = samplePoolManager;
            _samples = samples;

            _packingThreads = new Thread[nPackerThreads];
            for (var i = 0; i < nPackerThreads; i++)
            {
                _packingThreads[i] = new Thread(PackSamples);
                _packingThreads[i].Start();
            }
        }

        public void Enqueue(UnpackedSample unpackedSample)
        {
            _unpackedSamples.Enqueue(unpackedSample);
        }

        public void Stop()
        {
            _shouldStop = true;
        }

        public void Join()
        {
            foreach (var packingThread in _packingThreads)
            {
                packingThread.Join();
            }
        }

        private void PackSamples()
        {
            do
            {
                if (!_unpackedSamples.TryDequeue(out var unpackedSample)) continue;

                if (unpackedSample.Header != null)
                {
                    var sampleStamped = _samplePoolManager.GetPackedSampleStamped();
                    sampleStamped.Header.Seq = unpackedSample.Header.Seq;
                    sampleStamped.Header.Time = unpackedSample.Header.Time;
                    sampleStamped.Payload = Any.Pack(unpackedSample.Payload);
                    _samples.TryAdd(sampleStamped);
                    _samplePoolManager.ReleaseSamplePayload(unpackedSample.Payload);
                    _samplePoolManager.ReleaseUnpackedSampleStamped(unpackedSample);
                }
                else
                {
                    var sample = _samplePoolManager.GetPackedSample();
                    sample.Payload = Any.Pack(unpackedSample.Payload);
                    _samples.TryAdd(sample);
                    _samplePoolManager.ReleaseSamplePayload(unpackedSample.Payload);
                    _samplePoolManager.ReleaseUnpackedSample(unpackedSample);
                }
            } while (!_shouldStop || _unpackedSamples.Count > 0);
        }
    }
}