using System.Diagnostics;

namespace PLUME.Core.Recorder
{
    public class Clock
    {
        private readonly Stopwatch _clock = new();

        public void Start()
        {
            _clock.Start();
        }

        public void Restart()
        {
            _clock.Restart();
        }

        public void Stop()
        {
            _clock.Stop();
        }

        public void Reset()
        {
            _clock.Reset();
        }

        public long ElapsedNanoseconds => _clock.ElapsedTicks * (1_000_000_000 / Stopwatch.Frequency);
    }
}