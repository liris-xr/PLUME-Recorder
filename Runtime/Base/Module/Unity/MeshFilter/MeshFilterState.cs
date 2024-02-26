using PLUME.Sample.ProtoBurst;

namespace PLUME.Base.Module.Unity.MeshFilter
{
    internal struct MeshFilterState
    {
        public AssetIdentifier MeshIdentifier;
        public bool MeshIdentifierDirty;

        public AssetIdentifier SharedMeshIdentifier;
        public bool SharedMeshIdentifierDirty;
        
        public bool IsDirty => MeshIdentifierDirty || SharedMeshIdentifierDirty;
        
        public void MarkClean()
        {
            MeshIdentifierDirty = false;
            SharedMeshIdentifierDirty = false;
        }
    }
}