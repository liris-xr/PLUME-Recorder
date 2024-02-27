using PLUME.Core.Object.SafeRef;

namespace PLUME.Core.Recorder.Module
{
    public interface IObjectRecorderModule : IRecorderModule
    {
        public void StartRecordingObject(IObjectSafeRef objSafeRef, bool markCreated, Record record,
            RecorderContext ctx);

        public void StopRecordingObject(IObjectSafeRef objSafeRef, bool markDestroyed, Record record,
            RecorderContext ctx);

        public bool IsObjectSupported(IObjectSafeRef objSafeRef);

        public bool IsRecordingObject(IObjectSafeRef objSafeRef);
    }
}