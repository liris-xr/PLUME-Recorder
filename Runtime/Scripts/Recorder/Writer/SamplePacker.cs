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
        private readonly IProducerConsumerCollection<PackedSample> _packedSamples;
        private readonly BlockingCollection<UnpackedSample> _unpackedSamples = new();

        private bool _shouldStop;

        public SamplePacker(SamplePoolManager samplePoolManager,
            IProducerConsumerCollection<PackedSample> packedSamples, int nPackerThreads = 4)
        {
            _samplePoolManager = samplePoolManager;
            _packedSamples = packedSamples;

            _packingThreads = new Thread[nPackerThreads];
            for (var i = 0; i < nPackerThreads; i++)
            {
                _packingThreads[i] = new Thread(PackSamples);
                _packingThreads[i].Start();
            }
        }

        public void Enqueue(UnpackedSample sample)
        {
            _unpackedSamples.Add(sample);
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
                if (!_unpackedSamples.TryTake(out var unpackedSample, 100)) continue;
                var packedSample = _samplePoolManager.GetPackedSample();
                packedSample.Header.Seq = unpackedSample.Header.Seq;
                packedSample.Header.Time = unpackedSample.Header.Time;
                packedSample.Payload = Any.Pack(unpackedSample.Payload);
                _packedSamples.TryAdd(packedSample);
                _samplePoolManager.ReleaseSamplePayload(unpackedSample.Payload);
                _samplePoolManager.ReleaseUnpackedSample(unpackedSample);
            } while (!_shouldStop || _unpackedSamples.Count > 0);
        }
    }
}