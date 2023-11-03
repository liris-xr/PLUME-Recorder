using System;

namespace PLUME
{
    public class SampleKey : IComparable<SampleKey>
    {
        public readonly SampleHeaderKey Header;

        public SampleKey(SampleHeaderKey header)
        {
            Header = header;
        }

        public int CompareTo(SampleKey other)
        {
            if (Header == null)
                return -1;
            if (other.Header == null)
                return Header == null ? 0 : 1;
            return Header.CompareTo(other.Header);
        }
    }
    
    public class SampleHeaderKey : IComparable<SampleHeaderKey>
    {
        public readonly ulong Time;
        public readonly ulong Seq;

        public SampleHeaderKey(ulong time, ulong seq)
        {
            Time = time;
            Seq = seq;
        }

        public int CompareTo(SampleHeaderKey other)
        {
            var timestampComparison = Time.CompareTo(other.Time);
            return timestampComparison != 0 ? timestampComparison : Seq.CompareTo(other.Seq);
        }
    }
}