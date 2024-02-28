using PLUME.Base.Module.Unity.Transform.Sample;
using PLUME.Base.Module.Unity.Transform.State;
using PLUME.Core.Object;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace PLUME.Base.Module.Unity.Transform.Job
{
    [BurstCompile]
    internal struct SampleProducerJob : IJobParallelForBatch
    {
        [ReadOnly] public NativeArray<ComponentIdentifier> Identifiers;

        public NativeArray<TransformPositionState> AlignedPositionStates;
        public NativeArray<TransformHierarchyState> AlignedHierarchyStates;
        public NativeArray<TransformFlagsState> AlignedFlagsStates;

        [WriteOnly] public NativeList<TransformUpdate>.ParallelWriter UpdateSamples;
        [WriteOnly] public NativeList<TransformCreate>.ParallelWriter CreateSamples;
        [WriteOnly] public NativeList<TransformDestroy>.ParallelWriter DestroySamples;

        public void Execute(int startIndex, int count)
        {
            for (var idx = startIndex; idx < startIndex + count; ++idx)
            {
                var identifier = Identifiers[idx];
                var positionState = AlignedPositionStates[idx];
                var hierarchyState = AlignedHierarchyStates[idx];
                var flagsState = AlignedFlagsStates[idx];

                var createdInFrame = flagsState.IsCreatedInFrame;
                var destroyedInFrame = flagsState.IsDestroyedInFrame;

                if (destroyedInFrame)
                {
                    DestroySamples.AddNoResize(new TransformDestroy(identifier));
                    continue;
                }

                if (createdInFrame)
                {
                    CreateSamples.AddNoResize(new TransformCreate(identifier));
                }

                if (!createdInFrame && !positionState.IsDirty && !hierarchyState.IsDirty)
                    continue;

                var updateSample = new TransformUpdate(identifier);

                if (hierarchyState.ParentTransformIdDirty || createdInFrame)
                {
                    updateSample.SetParent(hierarchyState.ParentTransformId);
                }

                if (hierarchyState.SiblingIndexDirty || createdInFrame)
                {
                    updateSample.SetSiblingIndex(hierarchyState.SiblingIndex);
                }

                if (positionState.LocalPositionDirty || createdInFrame)
                {
                    updateSample.SetLocalPosition(positionState.LocalPosition);
                }

                if (positionState.LocalRotationDirty || createdInFrame)
                {
                    updateSample.SetLocalRotation(positionState.LocalRotation);
                }

                if (positionState.LocalScaleDirty || createdInFrame)
                {
                    updateSample.SetLocalScale(positionState.LocalScale);
                }

                UpdateSamples.AddNoResize(updateSample);

                positionState.MarkClean();
                hierarchyState.MarkClean();
                flagsState.MarkClean();
                AlignedPositionStates[idx] = positionState;
                AlignedHierarchyStates[idx] = hierarchyState;
                AlignedFlagsStates[idx] = flagsState;
            }
        }
    }
}