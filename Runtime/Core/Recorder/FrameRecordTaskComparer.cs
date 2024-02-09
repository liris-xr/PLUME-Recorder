using System.Collections.Generic;

namespace PLUME.Core.Recorder
{
    public class FrameRecordTaskComparer : IEqualityComparer<FrameRecorderTask>
    {
        public static FrameRecordTaskComparer Instance { get; } = new();

        public bool Equals(FrameRecorderTask x, FrameRecorderTask y)
        {
            return x.Frame == y.Frame;
        }

        public int GetHashCode(FrameRecorderTask task)
        {
            return task.Frame;
        }
    }
}