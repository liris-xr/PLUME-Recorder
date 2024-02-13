using PLUME.Core.Object.SafeRef;

namespace PLUME.Core.Recorder.Module
{
    public interface IRecorderModule
    {
        internal void Create(ObjectSafeRefProvider objectSafeRefProvider, SampleTypeUrlRegistry sampleTypeUrlRegistry);

        internal void Destroy();

        internal void Start();

        internal void Stop();

        internal void Reset();
    }
}