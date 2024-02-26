using PLUME.Sample.ProtoBurst;
using Unity.Burst;

namespace PLUME.Base.Module.Unity.Transform.State
{
    [BurstCompile]
    internal struct HierarchyState
    {
        public TransformGameObjectIdentifier ParentIdentifier;
        public bool ParentDirty;

        public int SiblingIndex;
        public bool SiblingIndexDirty;
        
        public bool IsDirty => ParentDirty || SiblingIndexDirty;
        
        public void MarkClean()
        {
            ParentDirty = false;
            SiblingIndexDirty = false;
        }
    }
}