using PLUME.Core.Object.SafeRef;

namespace PLUME.Core.Recorder.Module
{
    public interface IObjectRecorderModule : IRecorderModule
    {
        public void StartRecordingObject(IObjectSafeRef objSafeRef, bool markCreated, RecorderContext ctx);

        public void StopRecordingObject(IObjectSafeRef objSafeRef, bool markDestroyed, RecorderContext ctx);

        public bool IsObjectSupported(IObjectSafeRef objSafeRef);

        public bool IsRecordingObject(IObjectSafeRef objSafeRef);
    }
}