namespace PLUME
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