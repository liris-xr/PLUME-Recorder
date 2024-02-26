using PLUME.Base.Module.Unity.Transform.Sample;
using PLUME.Base.Module.Unity.Transform.State;
using PLUME.Core.Object;
using PLUME.Sample.ProtoBurst;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace PLUME.Base.Module.Unity.Transform.Job
{
    [BurstCompile]
    internal struct SampleProducerJob : IJobParallelForBatch
    {
        [ReadOnly] public NativeArray<ObjectIdentifier> TransformIdentifiers;
        [ReadOnly] public NativeHashMap<ObjectIdentifier, ObjectIdentifier> GameObjectsIdentifiers;

        public NativeHashMap<ObjectIdentifier, PositionState> PositionStates;
        public NativeHashMap<ObjectIdentifier, HierarchyState> HierarchyStates;

        [ReadOnly] public NativeHashSet<ObjectIdentifier>.ReadOnly CreatedInFrameIdentifiers;
        [ReadOnly] public NativeHashSet<ObjectIdentifier>.ReadOnly DestroyedInFrameIdentifiers;

        [WriteOnly] public NativeList<TransformUpdate>.ParallelWriter UpdateSamples;
        [WriteOnly] public NativeList<TransformCreate>.ParallelWriter CreateSamples;
        [WriteOnly] public NativeList<TransformDestroy>.ParallelWriter DestroySamples;

        public void Execute(int startIndex, int count)
        {
            for (var idx = startIndex; idx < startIndex + count; ++idx)
            {
                var transformIdentifier = TransformIdentifiers[idx];
                var gameObjectIdentifier = GameObjectsIdentifiers[transformIdentifier];
                var identifier = new TransformGameObjectIdentifier(transformIdentifier, gameObjectIdentifier);

                var hierarchyState = HierarchyStates[transformIdentifier];
                var positionState = PositionStates[transformIdentifier];
                var createdInFrame = CreatedInFrameIdentifiers.Contains(transformIdentifier);
                var destroyedInFrame = DestroyedInFrameIdentifiers.Contains(transformIdentifier);

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
                PositionStates[transformIdentifier] = positionState;
                HierarchyStates[transformIdentifier] = hierarchyState;

                UpdateSamples.AddNoResize(updateSample);
            }
        }
    }
}