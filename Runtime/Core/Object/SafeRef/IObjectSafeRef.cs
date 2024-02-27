namespace PLUME.Core.Object.SafeRef
{
    /// <summary>
    /// Stores a reference to a Unity object along with its identifier. When the object is
    /// destroyed, the identifier is still valid and can be used to identify the object. This is useful for recording
    /// data about objects that have been destroyed and for which the reference is no longer valid.
    /// </summary>
    public interface IObjectSafeRef
    {
    }

    public interface IObjectSafeRef<out TO, out TI> : IObjectSafeRef
        where TO : UnityEngine.Object
        where TI : unmanaged, IObjectIdentifier
    {
        public TO GetObject();

        public TI GetIdentifier();
    }
}