using System.Collections.Concurrent;
using System.Threading;
using Google.Protobuf.WellKnownTypes;
using PLUME.Sample;

namespace PLUME
{
    public class SampleStampedPacker
    {
        private readonly SamplePoolManager _samplePoolManager;

        private readonly Thread[] _packingThreads;
        private readonly OrderedSampleList _samples;
        private readonly BlockingCollection<UnpackedSampleStamped> _unpackedSamplesStamped = new();

        private bool _shouldStop;

        public SampleStampedPacker(SamplePoolManager samplePoolManager, OrderedSampleList samples, int nPackerThreads = 4)
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

        public void Enqueue(UnpackedSampleStamped unpackedSampleStamped)
        {
            _unpackedSamplesStamped.Add(unpackedSampleStamped);
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
                if (!_unpackedSamplesStamped.TryTake(out var unpackedSample, 100)) continue;
                var sampleStamped = _samplePoolManager.GetSampleStamped();
                sampleStamped.Header.Seq = unpackedSample.Header.Seq;
                sampleStamped.Header.Time = unpackedSample.Header.Time;
                sampleStamped.Payload = Any.Pack(unpackedSample.Payload);
                _samples.TryAdd(sampleStamped);
                _samplePoolManager.ReleaseSamplePayload(unpackedSample.Payload);
                _samplePoolManager.ReleaseUnpackedSample(unpackedSample);
            } while (!_shouldStop || _unpackedSamplesStamped.Count > 0);
        }
    }
}