using System;
using System.Collections.Generic;
using Google.Protobuf;
using JetBrains.Annotations;

namespace PLUME.Sample
{
    public class UnpackedSample : UnpackedSample<IMessage>
    {
    }

    public class UnpackedSample<T> where T : IMessage
    {
        [CanBeNull] public SampleHeader Header;
        public T Payload;

        private bool Equals(UnpackedSample<T> other)
        {
            return Equals(Header, other.Header) && EqualityComparer<T>.Default.Equals(Payload, other.Payload);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((UnpackedSample<T>)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Header, Payload);
        }
    }
}