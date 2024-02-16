using System.Collections.Generic;

namespace PLUME.Core.Recorder.Module
{
    public readonly struct Frame
    {
        public readonly long Timestamp;
        public readonly int FrameNumber;

        public Frame(long timestamp, int frameNumber)
        {
            Timestamp = timestamp;
            FrameNumber = frameNumber;
        }
    }

    public class FrameComparer : IComparer<Frame>, IEqualityComparer<Frame>
    {
        public static FrameComparer Instance { get; } = new();

        public int Compare(Frame x, Frame y)
        {
            return x.FrameNumber.CompareTo(y.FrameNumber);
        }

        public bool Equals(Frame x, Frame y)
        {
            return x.FrameNumber == y.FrameNumber;
        }

        public int GetHashCode(Frame obj)
        {
            return obj.FrameNumber;
        }
    }
}