using PLUME.Core.Object.SafeRef;

namespace PLUME.Core.Recorder.Module
{
    public interface IObjectRecorderModule : IRecorderModule
    {
        public bool TryStartRecordingObject(IObjectSafeRef obj, bool markCreated = true);

        public bool TryStopRecordingObject(IObjectSafeRef obj, bool markDestroyed = true);
    }
}