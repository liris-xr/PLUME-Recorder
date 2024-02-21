using System.Collections.Generic;

namespace PLUME.Core.Recorder.Module.Frame
{
    public readonly struct FrameInfo
    {
        public readonly long Timestamp;
        public readonly int FrameNumber;

        public FrameInfo(long timestamp, int frameNumber)
        {
            Timestamp = timestamp;
            FrameNumber = frameNumber;
        }
    }

    public class FrameInfoComparer : IComparer<FrameInfo>, IEqualityComparer<FrameInfo>
    {
        public static FrameInfoComparer Instance { get; } = new();

        public int Compare(FrameInfo x, FrameInfo y)
        {
            return x.FrameNumber.CompareTo(y.FrameNumber);
        }

        public bool Equals(FrameInfo x, FrameInfo y)
        {
            return x.FrameNumber == y.FrameNumber;
        }

        public int GetHashCode(FrameInfo obj)
        {
            return obj.FrameNumber;
        }
    }
}