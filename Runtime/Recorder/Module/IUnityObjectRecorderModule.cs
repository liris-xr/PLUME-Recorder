namespace PLUME.Recorder.Module
{
    public interface IUnityObjectRecorderModule : IUnityRecorderModule
    {
        public bool TryStartRecording(IObjectSafeRef obj, bool markCreated = true);

        public bool TryStopRecording(IObjectSafeRef obj, bool markDestroyed = true);
    }
}