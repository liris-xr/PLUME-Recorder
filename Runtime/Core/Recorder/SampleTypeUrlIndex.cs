using System;

namespace PLUME.Core.Recorder
{
    public readonly struct SampleTypeUrlIndex : IEquatable<SampleTypeUrlIndex>
    {
        private readonly int _index;

        internal SampleTypeUrlIndex(int index)
        {
            _index = index;
        }

        public bool Equals(SampleTypeUrlIndex other)
        {
            return _index == other._index;
        }

        public override bool Equals(object obj)
        {
            return obj is SampleTypeUrlIndex other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _index;
        }
    }
}