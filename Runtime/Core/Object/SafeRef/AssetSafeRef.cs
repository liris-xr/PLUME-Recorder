using Unity.Collections;
using UnityObject = UnityEngine.Object;

namespace PLUME.Core.Object.SafeRef
{
    public interface IAssetSafeRef : IObjectSafeRef<AssetIdentifier>
    {
        public UnityObject Asset { get; }
    }

    public interface IAssetSafeRef<out TA> : IAssetSafeRef where TA : UnityObject
    {
        public new TA Asset { get; }
    }

    // TODO: make this alloc free
    public class AssetSafeRef<TA> : IAssetSafeRef<TA> where TA : UnityObject
    {
        public static readonly AssetSafeRef<TA> Null = new();
        
        public TA Asset { get; }
        public AssetIdentifier Identifier { get; }

        UnityObject IAssetSafeRef.Asset => Asset;
        UnityObject IObjectSafeRef.Object => Asset;
        IObjectIdentifier IObjectSafeRef.Identifier => Identifier;

        public bool IsNull => Identifier.Equals(AssetIdentifier.Null);

        internal AssetSafeRef()
        {
            Asset = null;
            Identifier = AssetIdentifier.Null;
        }

        internal AssetSafeRef(TA asset, Guid guid, FixedString512Bytes assetPath)
        {
            Asset = asset;
            var identifier = new Identifier(asset.GetInstanceID(), guid);
            Identifier = new AssetIdentifier(identifier, assetPath);
        }

        public bool Equals(AssetSafeRef<TA> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Identifier.Equals(other.Identifier);
        }

        public bool Equals(IObjectSafeRef<AssetIdentifier> other)
        {
            return other is AssetSafeRef<TA> assetSafeRef && Equals(assetSafeRef);
        }

        public bool Equals(IObjectSafeRef other)
        {
            return other is AssetSafeRef<TA> assetSafeRef && Equals(assetSafeRef);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((AssetSafeRef<TA>)obj);
        }

        public override int GetHashCode()
        {
            return Identifier.GetHashCode();
        }
    }
}