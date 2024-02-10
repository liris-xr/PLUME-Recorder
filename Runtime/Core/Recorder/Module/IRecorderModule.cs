namespace PLUME.Core.Recorder.Module
{
    public interface IRecorderModule
    {
        internal void Create(PlumeRecorder recorder);
        
        internal void Destroy();
        
        internal void Start();

        internal void Stop();

        internal void Reset();
    }
}