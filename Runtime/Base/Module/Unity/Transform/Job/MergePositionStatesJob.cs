using PLUME.Base.Module.Unity.Transform.State;
using PLUME.Core.Object;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace PLUME.Base.Module.Unity.Transform.Job
{
    [BurstCompile]
    public struct MergePositionStatesJob : IJobParallelForBatch
    {
        [ReadOnly] public NativeArray<PositionState> PositionStatesWorkCopy;
        [ReadOnly] public NativeArray<ObjectIdentifier> IdentifiersWorkCopy;

        [NativeDisableParallelForRestriction]
        public NativeHashMap<ObjectIdentifier, PositionState> PositionStates;

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