using System;
using System.Collections.Generic;
using Google.Protobuf;

namespace PLUME.Sample
{
    public class UnpackedSample : UnpackedSample<IMessage>
    {
        protected UnpackedSample() {}
        
        public new static UnpackedSample InstantiateEmptyUnpackedSample()
        {
            return new UnpackedSample();
        }
        
        public new static UnpackedSample InstantiateUnpackedSample(SampleHeader header, IMessage payload)
        {
            var sample = new UnpackedSample();
            sample.Header = header;
            sample.Payload = payload;
            return sample;
        }
    }

    public class UnpackedSample<T> where T : IMessage
    {
        public SampleHeader Header;
        public T Payload;

        protected UnpackedSample() {}

        protected bool Equals(UnpackedSample<T> other)
        {
            return Equals(Header, other.Header) && EqualityComparer<T>.Default.Equals(Payload, other.Payload);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((UnpackedSample<T>) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Header, Payload);
        }

        public static UnpackedSample<T> InstantiateEmptyUnpackedSample()
        {
            return new UnpackedSample<T>();
        }
        
        public static UnpackedSample<T> InstantiateUnpackedSample(SampleHeader header, T payload)
        {
            var sample = new UnpackedSample<T>();
            sample.Header = header;
            sample.Payload = payload;
            return sample;
        }
    }
}