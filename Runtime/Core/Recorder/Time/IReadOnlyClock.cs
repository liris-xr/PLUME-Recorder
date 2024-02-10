namespace PLUME.Core.Recorder.Time
{
    public interface IReadOnlyClock
    {
        public bool IsRunning();
        
        public long ElapsedNanoseconds { get; }
    }
}