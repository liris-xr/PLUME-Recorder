using PLUME.Base.Module.Unity.Transform.State;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Jobs;

namespace PLUME.Base.Module.Unity.Transform.Job
{
    /// <summary>
    /// Update the LocalPosition, LocalScale and LocalRotation of the recorded transforms in their corresponding <see cref="TransformState"/>.
    /// </summary>
    [BurstCompile]
    public struct PollPositionStatesJob : IJobParallelForTransform
    {
        public float AngularThreshold; // in radians
        public float PositionThresholdSq;
        public float ScaleThresholdSq;
        
        public NativeArray<PositionState> PositionStates;
        
        public void Execute(int index, TransformAccess transform)
        {
            if(!transform.isValid)
                return;
            
            var state = PositionStates[index];

            var localPosition = (float3)transform.localPosition;
            var localRotation = (quaternion)transform.localRotation;
            var localScale = (float3)transform.localScale;

            var angle = MathUtils.Angle(state.LocalRotation, localRotation);

            var localPositionChanged = math.distancesq(state.LocalPosition, localPosition) >= PositionThresholdSq;
            var localScaleChanged = math.distancesq(state.LocalScale, localScale) >= ScaleThresholdSq;
            var localRotationChanged = angle >= AngularThreshold;
            
            var newPositionState = new PositionState
            {
                LocalPosition = localPosition,
                LocalRotation = localRotation,
                LocalScale = localScale,
                LocalPositionChanged = localPositionChanged,
                LocalRotationChanged = localRotationChanged,
                LocalScaleChanged = localScaleChanged
            };
            
            PositionStates[index] = newPositionState;
        }
    }
}