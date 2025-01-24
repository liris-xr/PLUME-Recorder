using PLUME.Core.Object;
using PLUME.Sample.ProtoBurst.Unity;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace PLUME.Base.Module.Unity.Transform
{
    [BurstCompile]
    [GenerateTestsForBurstCompatibility]
    internal struct TransformState
    {
        public LifeStatus Status;
        
        public ComponentIdentifier ParentTransformId;
        public bool ParentTransformIdDirty;

        public int SiblingIndex;
        public bool SiblingIndexDirty;

        public float3 LocalPosition;
        public bool LocalPositionDirty;

        public quaternion LocalRotation;
        public bool LocalRotationDirty;

        public float3 LocalScale;
        public bool LocalScaleDirty;

        public bool IsLocalTransformDirty => LocalPositionDirty || LocalRotationDirty || LocalScaleDirty;

        public bool IsHierarchyDirty => ParentTransformIdDirty || SiblingIndexDirty;

        public void MarkClean()
        {
            LocalPositionDirty = false;
            LocalRotationDirty = false;
            LocalScaleDirty = false;

            ParentTransformIdDirty = false;
            SiblingIndexDirty = false;
        }

        internal enum LifeStatus
        {
            Alive = 0,
            AliveCreatedInFrame = 1,
            DestroyedInFrame = 2
        }
    }
}