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

        private bool Equals(AssetSafeRef<TObject> other)
        {
            return Identifier.Equals(other.Identifier);
        }

        public bool Equals(IObjectSafeRef other)
        {
            return other is AssetSafeRef<TObject> assetSafeRef && Equals(assetSafeRef);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((AssetSafeRef<TObject>)obj);
        }

        public override int GetHashCode()
        {
            return Identifier.GetHashCode();
        }
    }
}