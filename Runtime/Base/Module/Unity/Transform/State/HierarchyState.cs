using PLUME.Sample.ProtoBurst;

namespace PLUME.Base.Module.Unity.Transform.State
{
    public struct HierarchyState
    {
        public TransformGameObjectIdentifier ParentIdentifier;
        public bool ParentChanged;
        
        public int SiblingIndex;
        public bool SiblingIndexChanged;
    }
}