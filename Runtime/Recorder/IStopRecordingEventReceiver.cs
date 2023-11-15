﻿namespace PLUME
{
    public interface IStopRecordingEventReceiver
    {
        public void OnStopRecording();
        
        public int ExecutionPriority()
        {
            return 1000;
        }
    }
}