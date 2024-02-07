namespace PLUME.Recorder
{
    public interface IStartRecordingEventReceiver
    {
        public void OnStartRecording();

        public int ExecutionPriority()
        {
            return 1000;
        }
    }
}