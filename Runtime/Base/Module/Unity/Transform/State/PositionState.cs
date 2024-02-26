using Unity.Burst;
using Unity.Mathematics;

namespace PLUME.Base.Module.Unity.Transform.State
{
    [BurstCompile]
    internal struct PositionState
    {
        public float3 LocalPosition;
        public bool LocalPositionDirty;

        public quaternion LocalRotation;
        public bool LocalRotationDirty;

        public float3 LocalScale;
        public bool LocalScaleDirty;

        public bool IsDirty => LocalPositionDirty || LocalRotationDirty || LocalScaleDirty;

        public void MarkClean()
        {
            LocalPositionDirty = false;
            LocalRotationDirty = false;
            LocalScaleDirty = false;
        }
    }
}