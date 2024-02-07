using System.Collections.Generic;
using PLUME.Sample;

namespace PLUME.Recorder.Writer
{
    public class UnpackedSampleComparer : IComparer<UnpackedSample>
    {
        public int Compare(UnpackedSample x, UnpackedSample y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;
            if (ReferenceEquals(null, y.Header)) return 1;
            if (ReferenceEquals(null, x.Header)) return -1;
            var timeComparison = x.Header.Time.CompareTo(y.Header.Time);
            return timeComparison != 0 ? timeComparison : x.Header.Seq.CompareTo(y.Header.Seq);
        }
    }
}