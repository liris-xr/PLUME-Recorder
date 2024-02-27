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

        public NativeHashMap<ComponentIdentifier, TransformPositionState> PositionStates;
        public NativeHashMap<ComponentIdentifier, TransformHierarchyState> HierarchyStates;

        [ReadOnly] public NativeHashSet<ComponentIdentifier>.ReadOnly CreatedInFrameIdentifiers;
        [ReadOnly] public NativeHashSet<ComponentIdentifier>.ReadOnly DestroyedInFrameIdentifiers;

        [WriteOnly] public NativeList<TransformUpdate>.ParallelWriter UpdateSamples;
        [WriteOnly] public NativeList<TransformCreate>.ParallelWriter CreateSamples;
        [WriteOnly] public NativeList<TransformDestroy>.ParallelWriter DestroySamples;

        public void Execute(int startIndex, int count)
        {
            for (var idx = startIndex; idx < startIndex + count; ++idx)
            {
                var identifier = Identifiers[idx];

                var hierarchyState = HierarchyStates[identifier];
                var positionState = PositionStates[identifier];
                var createdInFrame = CreatedInFrameIdentifiers.Contains(identifier);
                var destroyedInFrame = DestroyedInFrameIdentifiers.Contains(identifier);

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
                
                if(hierarchyState.ParentDirty || createdInFrame)
                {
                    updateSample.SetParent(hierarchyState.ParentIdentifier);
                }
                
                if(hierarchyState.SiblingIndexDirty || createdInFrame)
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
                
                positionState.MarkClean();
                hierarchyState.MarkClean();
                PositionStates[identifier] = positionState;
                HierarchyStates[identifier] = hierarchyState;

                UpdateSamples.AddNoResize(updateSample);
            }
        }
    }
}