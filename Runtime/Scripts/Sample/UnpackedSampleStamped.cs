using System;
using System.Collections.Generic;
using Google.Protobuf;

namespace PLUME.Sample
{
    public class UnpackedSampleStamped : UnpackedSampleStamped<IMessage>
    {
        public UnpackedSampleStamped()
        {
        }
    }

    public class UnpackedSampleStamped<T> where T : IMessage
    {
        public SampleHeader Header;
        public T Payload;

        public UnpackedSampleStamped()
        {
        }

        protected bool Equals(UnpackedSampleStamped<T> other)
        {
            return Equals(Header, other.Header) && EqualityComparer<T>.Default.Equals(Payload, other.Payload);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((UnpackedSampleStamped<T>) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Header, Payload);
        }
    }
}