namespace PLUME.Recorder.Module
{
    public interface IRecorderModule
    {
        internal void Create();
        
        internal void Destroy();
        
        internal void Start();

        internal void Stop();

        internal void Reset();
    }
}