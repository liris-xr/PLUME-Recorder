using Unity.Collections;
using UnityObject = UnityEngine.Object;

namespace PLUME.Core.Object.SafeRef
{
    public class AssetSafeRef<TObject> : IObjectSafeRef<TObject, AssetIdentifier> where TObject : UnityObject
    {
        public static readonly AssetSafeRef<TObject> Null = new();

        public readonly AssetIdentifier Identifier;
        
        public readonly TObject Asset;

        private AssetSafeRef()
        {
            Asset = null;
            Identifier = AssetIdentifier.Null;
        }

        internal AssetSafeRef(TObject asset, Guid guid, FixedString512Bytes assetPath)
        {
            Asset = asset;
            Identifier = new AssetIdentifier(new Identifier(asset.GetInstanceID(), guid), assetPath);
        }

        public TObject GetObject()
        {
            return Asset;
        }

        public AssetIdentifier GetIdentifier()
        {
            return Identifier;
        }
    }
}