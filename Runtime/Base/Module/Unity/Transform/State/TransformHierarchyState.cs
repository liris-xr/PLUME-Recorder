using Unity.Burst;
using ComponentIdentifier = PLUME.Core.Object.ComponentIdentifier;

namespace PLUME.Base.Module.Unity.Transform.State
{
    [BurstCompile]
    internal struct TransformHierarchyState
    {
        public ComponentIdentifier ParentTransformId;
        public bool ParentTransformIdDirty;

        public int SiblingIndex;
        public bool SiblingIndexDirty;

        public bool IsDirty => ParentTransformIdDirty || SiblingIndexDirty;

        public void MarkClean()
        {
            ParentTransformIdDirty = false;
            SiblingIndexDirty = false;
        }
    }
}