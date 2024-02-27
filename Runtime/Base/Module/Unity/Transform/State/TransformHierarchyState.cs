using Unity.Burst;
using ComponentIdentifier = PLUME.Core.Object.ComponentIdentifier;

namespace PLUME.Base.Module.Unity.Transform.State
{
    [BurstCompile]
    internal struct TransformHierarchyState
    {
        public ComponentIdentifier ParentIdentifier;
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