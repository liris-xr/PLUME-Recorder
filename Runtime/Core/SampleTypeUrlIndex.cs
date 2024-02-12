using System;

namespace PLUME.Core
{
    /// <summary>
    /// A unique identifier associated to a ProtoBuf sample type URL (eg. "type.googleapis.com/google.protobuf.Any").
    /// This is used in <see cref="SampleTypeUrlRegistry"/> to map this unique identifier to a sample type URL string and
    /// thus prevent storing a full string for each sample stored in a <see cref="Recorder.SerializedSamplesBuffer"/>.
    /// </summary>
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