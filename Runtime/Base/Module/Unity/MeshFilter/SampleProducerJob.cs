using PLUME.Base.Module.Unity.MeshFilter.Sample;
using PLUME.Core.Object;
using PLUME.Sample.ProtoBurst;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace PLUME.Base.Module.Unity.MeshFilter
{
    [BurstCompile]
    internal struct SampleProducerJob : IJobParallelForBatch
    {
        [ReadOnly] public NativeArray<ObjectIdentifier> Identifiers;
        [ReadOnly] public NativeHashMap<ObjectIdentifier, ObjectIdentifier> ParentIdentifiers;

        public NativeHashMap<ObjectIdentifier, MeshFilterState> States;

        [ReadOnly] public NativeHashSet<ObjectIdentifier>.ReadOnly CreatedInFrameIdentifiers;
        [ReadOnly] public NativeHashSet<ObjectIdentifier>.ReadOnly DestroyedInFrameIdentifiers;

        [WriteOnly] public NativeList<MeshFilterUpdate>.ParallelWriter UpdateSamples;
        [WriteOnly] public NativeList<MeshFilterCreate>.ParallelWriter CreateSamples;
        [WriteOnly] public NativeList<MeshFilterDestroy>.ParallelWriter DestroySamples;

        public void Execute(int startIndex, int count)
        {
            for (var idx = startIndex; idx < startIndex + count; ++idx)
            {
                var identifier = Identifiers[idx];
                var parentIdentifier = ParentIdentifiers[identifier];
                var componentIdentifier = new ComponentIdentifier(identifier, parentIdentifier);
                
                var state = States[identifier];
                var createdInFrame = CreatedInFrameIdentifiers.Contains(identifier);
                var destroyedInFrame = DestroyedInFrameIdentifiers.Contains(identifier);

                if (destroyedInFrame)
                {
                    DestroySamples.AddNoResize(new MeshFilterDestroy(componentIdentifier));
                    continue;
                }

                if (createdInFrame)
                {
                    CreateSamples.AddNoResize(new MeshFilterCreate(componentIdentifier));
                }

                if (!createdInFrame && !state.IsDirty)
                    continue;

                var updateSample = new MeshFilterUpdate(componentIdentifier);
                
                if(state.MeshIdentifierDirty || createdInFrame)
                {
                    updateSample.SetMeshIdentifier(state.MeshIdentifier);
                }
                
                if(state.SharedMeshIdentifierDirty || createdInFrame)
                {
                    updateSample.SetSharedMeshIdentifier(state.SharedMeshIdentifier);
                }
                
                state.MarkClean();
                States[identifier] = state;

                UpdateSamples.AddNoResize(updateSample);
            }
        }
    }
}