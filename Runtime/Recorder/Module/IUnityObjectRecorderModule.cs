namespace PLUME.Recorder.Module
{
    public interface IUnityObjectRecorderModule : IRecorderModule
    {
        public bool TryStartRecordingObject(IObjectSafeRef obj, bool markCreated = true);

        public bool TryStopRecordingObject(IObjectSafeRef obj, bool markDestroyed = true);
    }
}