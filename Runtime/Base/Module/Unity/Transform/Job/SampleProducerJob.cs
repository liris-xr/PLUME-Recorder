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
    public struct SampleProducerJob : IJobParallelForBatch
    {
        [ReadOnly] public NativeList<ObjectIdentifier> TransformIdentifiers;
        [ReadOnly] public NativeHashMap<ObjectIdentifier, ObjectIdentifier> GameObjectsIdentifiers;
        
        [ReadOnly] public NativeHashMap<ObjectIdentifier, PositionState> PositionStates;
        [ReadOnly] public NativeHashMap<ObjectIdentifier, HierarchyState> HierarchyStates;
        
        [ReadOnly] public NativeHashSet<ObjectIdentifier>.ReadOnly CreatedInFrame;
        [ReadOnly] public NativeHashSet<ObjectIdentifier>.ReadOnly DestroyedInFrame;

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
                var createdInFrame = CreatedInFrame.Contains(transformIdentifier);
                var destroyedInFrame = DestroyedInFrame.Contains(transformIdentifier);

                if (destroyedInFrame)
                {
                    DestroySamples.AddNoResize(new TransformDestroy(identifier));
                    continue;
                }
                
                if (createdInFrame)
                {
                    CreateSamples.AddNoResize(new TransformCreate(identifier));
                }
                
                if(!createdInFrame && !positionState.HasChanged)
                    continue;
                    
                var updateSample = new TransformUpdate(identifier);
                
                if (positionState.LocalPositionChanged || createdInFrame)
                {
                    updateSample.SetLocalPosition(positionState.LocalPosition);
                }
                
                if (positionState.LocalRotationChanged || createdInFrame)
                {
                    updateSample.SetLocalRotation(positionState.LocalRotation);
                }
                
                if (positionState.LocalScaleChanged || createdInFrame)
                {
                    updateSample.SetLocalScale(positionState.LocalScale);
                }
                
                UpdateSamples.AddNoResize(updateSample);
            }
        }
    }
}