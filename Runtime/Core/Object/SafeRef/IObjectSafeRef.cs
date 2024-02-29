using System;

namespace PLUME.Core.Object.SafeRef
{
    /// <summary>
    /// Stores a reference to a Unity object along with its identifier. When the object is
    /// destroyed, the identifier is still valid and can be used to identify the object. This is useful for recording
    /// data about objects that have been destroyed and for which the reference is no longer valid.
    /// </summary>
    public interface IObjectSafeRef : IEquatable<IObjectSafeRef>
    {
        public IObjectIdentifier Identifier { get; }

        public UnityEngine.Object Object { get; }

        public bool IsNull { get; }
    }
    
    public interface IObjectSafeRef<out TObjectIdentifier> : IObjectSafeRef
        where TObjectIdentifier : unmanaged, IObjectIdentifier
    {
        public new TObjectIdentifier Identifier { get; }
    }
}