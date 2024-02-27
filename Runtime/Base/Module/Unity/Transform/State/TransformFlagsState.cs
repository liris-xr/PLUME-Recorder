using Unity.Burst;

namespace PLUME.Base.Module.Unity.Transform.State
{
    [BurstCompile]
    internal struct TransformFlagsState
    {
        public const int CreatedInFrameFlag = 1;
        public const int DestroyedInFrameFlag = 2;

        private byte _flags;

        public bool IsCreatedInFrame => (_flags & CreatedInFrameFlag) == CreatedInFrameFlag;
        
        public bool IsDestroyedInFrame => (_flags & DestroyedInFrameFlag) == DestroyedInFrameFlag;

        public void MarkCreatedInFrame()
        {
            _flags |= CreatedInFrameFlag;
        }

        public void MarkDestroyedInFrame()
        {
            _flags |= DestroyedInFrameFlag;
        }

        public void MarkClean()
        {
            _flags = 0;
        }
    }
}