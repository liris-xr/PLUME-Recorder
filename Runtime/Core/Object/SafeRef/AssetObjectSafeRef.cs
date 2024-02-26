
using Unity.Collections;
using Guid = PLUME.Sample.ProtoBurst.Guid;
using UnityObject = UnityEngine.Object;

namespace PLUME.Core.Object.SafeRef
{
    public class AssetObjectSafeRef<TObject> : ObjectSafeRef<TObject> where TObject : UnityObject
    {
        public static AssetObjectSafeRef<TObject> Null { get; } = new(ObjectIdentifier.Null);
        
        public readonly FixedString512Bytes AssetPath;
        
        private AssetObjectSafeRef(ObjectIdentifier identifier) : base(identifier)
        {
            AssetPath = "";
        }
        
        public AssetObjectSafeRef(TObject @object, Guid guid, FixedString512Bytes assetPath) : base(@object, guid)
        {
            AssetPath = assetPath;
        }
    }
}