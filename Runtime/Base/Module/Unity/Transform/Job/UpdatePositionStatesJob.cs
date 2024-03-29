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
    internal struct UpdatePositionStatesJob : IJobParallelForTransform
    {
        public float AngularThreshold; // in radians
        public float PositionThresholdSq;
        public float ScaleThresholdSq;

        public NativeArray<TransformState> AlignedStates;

        public void Execute(int index, TransformAccess transform)
        {
            if (!transform.isValid)
                return;

            var state = AlignedStates[index];

            var localPosition = (float3)transform.localPosition;
            var localRotation = (quaternion)transform.localRotation;
            var localScale = (float3)transform.localScale;

            var angle = MathUtils.Angle(state.LocalRotation, localRotation);

            var localPositionChanged = math.distancesq(state.LocalPosition, localPosition) >= PositionThresholdSq;
            var localScaleChanged = math.distancesq(state.LocalScale, localScale) >= ScaleThresholdSq;
            var localRotationChanged = angle >= AngularThreshold;

            state.LocalPosition = localPosition;
            state.LocalRotation = localRotation;
            state.LocalScale = localScale;
            state.LocalPositionDirty = localPositionChanged;
            state.LocalRotationDirty = localRotationChanged;
            state.LocalScaleDirty = localScaleChanged;
            AlignedStates[index] = state;
        }
    }
}