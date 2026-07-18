using System;
using System.Collections.Generic;
using PLUME.Base.Settings;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.ProtoBurst.Unity;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Scripting;
using SkinnedMeshRendererSafeRef = PLUME.Core.Object.SafeRef.IComponentSafeRef<UnityEngine.SkinnedMeshRenderer>;

namespace PLUME.Base.Module.Unity.Renderer.SkinnedMeshRendererModule
{
    /// <summary>
    /// Records skinned mesh renderer blend shape weights by polling every recorded renderer once per frame on a
    /// dedicated, allocation-free stream (decoupled from <see cref="PLUME.Sample.Unity.SkinnedMeshRendererUpdate"/>).
    /// Polling (rather than relying on the <c>SkinnedMeshRenderer.SetBlendShapeWeight</c> hook) is required because
    /// an <see cref="Animator"/> writes blend shape weights internally through the playable-graph binding, bypassing
    /// the managed setter and its hook entirely.
    /// <para>
    /// Each frame the current weights are compared against the last recorded weights: a renderer only emits a
    /// <see cref="SkinnedMeshRendererBlendShapeUpdate"/> when at least one
    /// weight moved by more than <see cref="SkinnedMeshRendererBlendShapeRecorderModuleSettings.WeightThreshold"/>
    /// (or on its first recorded frame, or when its mesh's blend shape count changes). The reused buffers make the
    /// steady state allocation-free, so a face rig animating every frame produces no managed GC allocation.
    /// </para>
    /// </summary>
    [Preserve]
    public class SkinnedMeshRendererBlendShapeRecorderModule :
        ComponentRecorderModule<SkinnedMeshRenderer, SkinnedMeshRendererBlendShapeFrameData>
    {
        // Last recorded weights per recorded renderer. Each buffer is allocated once when the renderer starts
        // being recorded (or reallocated when its blend shape count changes) and reused every frame, so no managed
        // allocation happens in steady state.
        private readonly Dictionary<SkinnedMeshRendererSafeRef, float[]> _previousWeights = new();

        private float _weightThreshold;

        protected override void OnCreate(RecorderContext ctx)
        {
            base.OnCreate(ctx);
            var settings = ctx.SettingsProvider.GetOrCreate<SkinnedMeshRendererBlendShapeRecorderModuleSettings>();
            _weightThreshold = settings.WeightThreshold;
        }

        protected override void OnStopRecordingObject(SkinnedMeshRendererSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnStopRecordingObject(objSafeRef, ctx);
            _previousWeights.Remove(objSafeRef);
        }

        protected override void OnStopRecording(RecorderContext ctx)
        {
            base.OnStopRecording(ctx);
            _previousWeights.Clear();
        }

        protected override SkinnedMeshRendererBlendShapeFrameData CollectFrameData(FrameInfo frameInfo,
            RecorderContext ctx)
        {
            var recorded = RecordedComponents;

            // Allocated lazily on the first changed renderer so that a frame in which nothing moves (the common
            // case for an idle avatar) does no allocation at all — native or managed. Left as default/un-created
            // when nothing changes; the frame data handles un-created lists in Serialize/Dispose.
            var updateSamples = default(NativeList<SkinnedMeshRendererBlendShapeUpdate>);
            var weights = default(NativeList<float>);

            for (var r = 0; r < recorded.Count; r++)
            {
                var objSafeRef = recorded[r];

                var skinnedMeshRenderer = objSafeRef.Component;

                if (skinnedMeshRenderer == null)
                    continue;

                var sharedMesh = skinnedMeshRenderer.sharedMesh;

                if (sharedMesh == null)
                    continue;

                var count = sharedMesh.blendShapeCount;

                if (count <= 0)
                    continue;

                // Fetch/allocate this renderer's previous-weights buffer. A missing buffer means the renderer is
                // newly recorded; a size mismatch means its mesh was swapped. Either way, force an update.
                var hasPrevious = _previousWeights.TryGetValue(objSafeRef, out var previous);
                var changed = !hasPrevious || previous.Length != count;

                if (changed)
                {
                    previous = new float[count];
                    _previousWeights[objSafeRef] = previous;
                }

                // Compare against the last recorded weights without touching the baseline, so a weight drifting
                // slower than the threshold per frame still accumulates and is eventually recorded instead of being
                // dropped frame after frame.
                if (!changed)
                {
                    for (var i = 0; i < count; i++)
                    {
                        if (Math.Abs(skinnedMeshRenderer.GetBlendShapeWeight(i) - previous[i]) > _weightThreshold)
                        {
                            changed = true;
                            break;
                        }
                    }
                }

                if (!changed)
                    continue;

                if (!weights.IsCreated)
                {
                    // First change this frame: allocate the shared buffers now (and only now).
                    updateSamples =
                        new NativeList<SkinnedMeshRendererBlendShapeUpdate>(recorded.Count - r, Allocator.Persistent);
                    // Shared buffer holding every changed renderer's weights back to back; samples index into it by
                    // offset/count.
                    weights = new NativeList<float>(Allocator.Persistent);
                }

                // Record current weights and advance the baseline to them, so the next comparison is against the
                // last recorded value.
                var offset = weights.Length;
                for (var i = 0; i < count; i++)
                {
                    var weight = skinnedMeshRenderer.GetBlendShapeWeight(i);
                    previous[i] = weight;
                    weights.Add(weight);
                }

                updateSamples.Add(new SkinnedMeshRendererBlendShapeUpdate
                {
                    Component = objSafeRef.Identifier,
                    WeightsOffset = offset,
                    WeightsCount = count
                });
            }

            return new SkinnedMeshRendererBlendShapeFrameData(updateSamples, weights);
        }
    }
}
