namespace PLUME
{
    public interface IStopRecordingObjectEventReceiver
    {
        public void OnStopRecordingObject(int objectInstanceId);
        
        public int ExecutionPriority()
        {
            return 1000;
        }
    }
}