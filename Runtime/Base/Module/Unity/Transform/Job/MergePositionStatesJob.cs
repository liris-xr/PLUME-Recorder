using PLUME.Base.Module.Unity.Transform.State;
using PLUME.Core.Object;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace PLUME.Base.Module.Unity.Transform.Job
{
    [BurstCompile]
    internal struct MergePositionStatesJob : IJobParallelForBatch
    {
        [ReadOnly] public NativeArray<TransformPositionState> PositionStatesWorkCopy;
        [ReadOnly] public NativeArray<ComponentIdentifier> IdentifiersWorkCopy;

        [NativeDisableParallelForRestriction]
        public NativeHashMap<ComponentIdentifier, TransformPositionState> PositionStates;

        public void Execute(int startIndex, int count)
        {
            for (var i = startIndex; i < startIndex + count; i++)
            {
                var identifier = IdentifiersWorkCopy[i];
                var positionState = PositionStatesWorkCopy[i];

                if (PositionStates.ContainsKey(identifier))
                {
                    PositionStates[identifier] = positionState;
                }
            }
        }
    }
}