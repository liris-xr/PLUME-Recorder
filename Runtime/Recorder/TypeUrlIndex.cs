using System;

namespace PLUME.Recorder
{
    public readonly struct TypeUrlIndex : IEquatable<TypeUrlIndex>
    {
        private readonly int _index;

        internal TypeUrlIndex(int index)
        {
            _index = index;
        }

        public bool Equals(TypeUrlIndex other)
        {
            return _index == other._index;
        }

        public override bool Equals(object obj)
        {
            return obj is TypeUrlIndex other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _index;
        }
    }
}