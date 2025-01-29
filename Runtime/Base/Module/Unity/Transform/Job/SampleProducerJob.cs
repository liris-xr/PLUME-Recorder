using PLUME.Core.Object;
using PLUME.Sample.ProtoBurst.Unity;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace PLUME.Base.Module.Unity.Transform.Job
{
    [BurstCompile]
    internal struct SampleProducerJob : IJobParallelForBatch
    {
        [ReadOnly] public NativeArray<ComponentIdentifier> Identifiers;

        public NativeArray<TransformState> AlignedStates;

        [WriteOnly] public NativeList<TransformUpdate>.ParallelWriter UpdateSamples;
        [WriteOnly] public NativeList<TransformCreate>.ParallelWriter CreateSamples;
        [WriteOnly] public NativeList<TransformDestroy>.ParallelWriter DestroySamples;

        public void Execute(int startIndex, int count)
        {
            for (var idx = startIndex; idx < startIndex + count; ++idx)
            {
                var identifier = Identifiers[idx];
                var state = AlignedStates[idx];

                var createdInFrame = state.Status == TransformState.LifeStatus.AliveCreatedInFrame;
                var destroyedInFrame = state.Status == TransformState.LifeStatus.DestroyedInFrame;

                if (destroyedInFrame)
                {
                    DestroySamples.AddNoResize(new TransformDestroy(identifier));
                    continue;
                }
                
                if (createdInFrame)
                {
                    CreateSamples.AddNoResize(new TransformCreate(identifier));
                }

                if (!createdInFrame && !state.IsLocalTransformDirty && !state.IsHierarchyDirty)
                    continue;

                var updateSample = new TransformUpdate(identifier);

                if (state.ParentTransformIdDirty || createdInFrame)
                {
                    updateSample.SetParent(state.ParentTransformId);
                }

                if (state.SiblingIndexDirty || createdInFrame)
                {
                    updateSample.SetSiblingIndex(state.SiblingIndex);
                }

                if (state.LocalPositionDirty || createdInFrame)
                {
                    updateSample.SetLocalPosition(state.LocalPosition);
                }

                if (state.LocalRotationDirty || createdInFrame)
                {
                    updateSample.SetLocalRotation(state.LocalRotation);
                }

                if (state.LocalScaleDirty || createdInFrame)
                {
                    updateSample.SetLocalScale(state.LocalScale);
                }

                UpdateSamples.AddNoResize(updateSample);

                state.MarkClean();
                state.Status = TransformState.LifeStatus.Alive;
                
                AlignedStates[idx] = state;
            }
        }
    }
}