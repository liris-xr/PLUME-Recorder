using PLUME.Core.Object;
using PLUME.Sample.ProtoBurst;
using Unity.Burst;
using Unity.Collections;
using UnityEngine.Jobs;

namespace PLUME.Base.Module.Unity.Transform
{
    [BurstCompile]
    public struct PollTransformStatesJob : IJobParallelForTransform
    {
        [ReadOnly] public NativeArray<ObjectIdentifier>.ReadOnly AlignedIdentifiers;

        [WriteOnly] public NativeList<TransformUpdateLocalPositionSample>.ParallelWriter DirtySamples;

        [NativeDisableParallelForRestriction] public NativeHashMap<ObjectIdentifier, TransformState> LastStates;

        public void Execute(int index, TransformAccess transform)
        {
            var identifier = AlignedIdentifiers[index];
            var lastSample = LastStates[identifier];

            if (lastSample.IsNull)
            {
                var state = new TransformState
                {
                    Identifier = identifier,
                    LocalPosition = transform.localPosition,
                    LocalRotation = transform.localRotation,
                    LocalScale = transform.localScale
                };

                var sample = new TransformUpdateLocalPositionSample
                {
                    LocalPosition = new Vector3Sample
                    {
                        X = state.LocalPosition.x,
                        Y = state.LocalPosition.y,
                        Z = state.LocalPosition.z
                    }
                };

                DirtySamples.AddNoResize(sample);
                LastStates[identifier] = state;
            }
            else
            {
                var localPosition = transform.localPosition;
                var localRotation = transform.localRotation;
                var localScale = transform.localScale;

                // TODO: add a distance threshold
                var isLocalPositionDirty = lastSample.LocalPosition != localPosition;
                var isLocalRotationDirty = lastSample.LocalRotation != localRotation;
                var isLocalScaleDirty = lastSample.LocalScale != localScale;

                if (!isLocalPositionDirty && !isLocalRotationDirty && !isLocalScaleDirty)
                    return;

                var state = new TransformState
                {
                    Identifier = identifier,
                    LocalPosition = localPosition,
                    LocalRotation = localRotation,
                    LocalScale = localScale
                };

                var sample = new TransformUpdateLocalPositionSample
                {
                    LocalPosition = new Vector3Sample
                    {
                        X = state.LocalPosition.x,
                        Y = state.LocalPosition.y,
                        Z = state.LocalPosition.z
                    }
                };

                DirtySamples.AddNoResize(sample);
                LastStates[identifier] = state;
            }
        }
    }
}