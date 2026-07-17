using System.Collections.Generic;
using PLUME.Base.Hooks;
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
    /// Records skinned mesh renderer blend shape weights on a dedicated, allocation-free stream (decoupled from
    /// <see cref="PLUME.Sample.Unity.SkinnedMeshRendererUpdate"/>). Weights are snapshotted once per frame into
    /// unmanaged <see cref="SkinnedMeshRendererBlendShapeUpdate"/> samples and serialized by a Burst job, so a
    /// face rig animating every frame produces no managed GC allocation.
    /// </summary>
    [Preserve]
    public class SkinnedMeshRendererBlendShapeRecorderModule :
        ComponentRecorderModule<SkinnedMeshRenderer, SkinnedMeshRendererBlendShapeFrameData>
    {
        // Renderers whose blend shape weights changed this frame (or that just started being recorded).
        private readonly HashSet<SkinnedMeshRendererSafeRef> _dirty = new();

        protected override void OnCreate(RecorderContext ctx)
        {
            base.OnCreate(ctx);
            SkinnedMeshRendererHooks.OnBlendShapeWeightChanged += (smr, _, _) => OnBlendShapeWeightChanged(smr, ctx);
        }

        protected override void OnObjectMarkedCreated(SkinnedMeshRendererSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);
            // Record the initial weights on the frame the renderer starts being recorded.
            _dirty.Add(objSafeRef);
        }

        private void OnBlendShapeWeightChanged(SkinnedMeshRenderer skinnedMeshRenderer, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.SafeRefProvider.GetOrCreateComponentSafeRef(skinnedMeshRenderer);

            if (!IsRecordingObject(objSafeRef))
                return;

            _dirty.Add(objSafeRef);
        }

        protected override SkinnedMeshRendererBlendShapeFrameData CollectFrameData(FrameInfo frameInfo,
            RecorderContext ctx)
        {
            var updateSamples =
                new NativeList<SkinnedMeshRendererBlendShapeUpdate>(_dirty.Count, Allocator.Persistent);
            // Shared buffer holding every renderer's weights back to back; samples index into it by offset/count.
            var weights = new NativeList<float>(Allocator.Persistent);

            foreach (var objSafeRef in _dirty)
            {
                if (!IsRecordingObject(objSafeRef) || objSafeRef.Component == null)
                    continue;

                var skinnedMeshRenderer = objSafeRef.Component;
                var sharedMesh = skinnedMeshRenderer.sharedMesh;

                if (sharedMesh == null)
                    continue;

                var count = sharedMesh.blendShapeCount;

                if (count <= 0)
                    continue;

                var offset = weights.Length;
                for (var i = 0; i < count; i++)
                {
                    weights.Add(skinnedMeshRenderer.GetBlendShapeWeight(i));
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

        protected override void AfterCollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            base.AfterCollectFrameData(frameInfo, ctx);
            _dirty.Clear();
        }
    }
}
