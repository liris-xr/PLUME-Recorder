using Unity.Collections;
using UnityObject = UnityEngine.Object;

namespace PLUME.Core.Object.SafeRef
{
    public class AssetSafeRef<TObject> : IObjectSafeRef<TObject, AssetIdentifier> where TObject : UnityObject
    {
        public static AssetSafeRef<TObject> Null { get; } = new(null, AssetIdentifier.Null);

        public AssetIdentifier Identifier { get; }
        public TObject Asset { get; }

        internal AssetSafeRef(TObject asset, AssetIdentifier identifier)
        {
            Asset = asset;
            Identifier = identifier;
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