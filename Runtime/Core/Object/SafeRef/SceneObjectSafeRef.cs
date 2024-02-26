
using PLUME.Sample.ProtoBurst;
using UnityObject = UnityEngine.Object;

namespace PLUME.Core.Object.SafeRef
{
    public class SceneObjectSafeRef<TObject> : ObjectSafeRef<TObject> where TObject : UnityObject
    {
        public static SceneObjectSafeRef<TObject> Null { get; } = new(ObjectIdentifier.Null);
        
        private SceneObjectSafeRef(ObjectIdentifier identifier) : base(identifier)
        {
        }
        
        public SceneObjectSafeRef(TObject @object, Guid guid) : base(@object, guid)
        {
        }
    }
}