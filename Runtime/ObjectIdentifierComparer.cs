using System.Collections.Generic;

namespace PLUME
{
    public sealed class ObjectIdentifierComparer : IEqualityComparer<ObjectIdentifier>
    {
        public static IEqualityComparer<ObjectIdentifier> Instance { get; } = new ObjectIdentifierComparer();

        public bool Equals(ObjectIdentifier x, ObjectIdentifier y)
        {
            return x.InstanceId == y.InstanceId;
        }

        public int GetHashCode(ObjectIdentifier identifier)
        {
            return identifier.InstanceId;
        }
    }
}