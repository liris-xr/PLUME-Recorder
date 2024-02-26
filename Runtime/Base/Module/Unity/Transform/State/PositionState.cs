using Unity.Mathematics;

namespace PLUME.Base.Module.Unity.Transform.State
{
    public struct PositionState
    {
        public float3 LocalPosition;
        public bool LocalPositionChanged;

        public quaternion LocalRotation;
        public bool LocalRotationChanged;

        public float3 LocalScale;
        public bool LocalScaleChanged;
        
        public bool HasChanged => LocalPositionChanged || LocalRotationChanged || LocalScaleChanged;

        public void CleanUp()
        {
            LocalPositionChanged = false;
            LocalRotationChanged = false;
            LocalScaleChanged = false;
        }
    }
}