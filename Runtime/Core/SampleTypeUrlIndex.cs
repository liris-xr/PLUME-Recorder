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
        public readonly int Index;

        internal SampleTypeUrlIndex(int index)
        {
            Index = index;
        }

        public bool Equals(SampleTypeUrlIndex other)
        {
            return Index == other.Index;
        }

        public override bool Equals(object obj)
        {
            return obj is SampleTypeUrlIndex other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Index;
        }
        
        public override string ToString()
        {
            return $"SampleTypeUrlIndex({Index})";
        }
    }
}