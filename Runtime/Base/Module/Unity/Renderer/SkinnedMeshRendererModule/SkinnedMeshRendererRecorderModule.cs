using System.Collections.Generic;
using PLUME.Base.Hooks;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity;
using UnityEngine;
using UnityEngine.Scripting;
using static PLUME.Core.Utils.SampleUtils;
using SkinnedMeshRendererSafeRef = PLUME.Core.Object.SafeRef.IComponentSafeRef<UnityEngine.SkinnedMeshRenderer>;

namespace PLUME.Base.Module.Unity.Renderer.SkinnedMeshRendererModule
{
    [Preserve]
    public class
        SkinnedMeshRendererRecorderModule : RendererRecorderModule<SkinnedMeshRenderer, SkinnedMeshRendererFrameData>
    {
        private readonly Dictionary<SkinnedMeshRendererSafeRef, SkinnedMeshRendererCreate> _createSamples = new();
        private readonly Dictionary<SkinnedMeshRendererSafeRef, SkinnedMeshRendererDestroy> _destroySamples = new();
        private readonly Dictionary<SkinnedMeshRendererSafeRef, SkinnedMeshRendererUpdate> _updateSamples = new();

        protected override void OnCreate(RecorderContext ctx)
        {
            base.OnCreate(ctx);

            SkinnedMeshRendererHooks.OnRootBoneChanged += (mr, rootBone) => OnRootBoneChanged(mr, rootBone, ctx);
            SkinnedMeshRendererHooks.OnBonesChanged += (mr, bones) => OnBonesChanged(mr, bones, ctx);
            SkinnedMeshRendererHooks.OnBlendShapeWeightChanged +=
                (mr, index, value) => OnBlendShapeWeightChanged(mr, ctx);
        }

        protected override void OnObjectMarkedCreated(SkinnedMeshRendererSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);

            var meshSafeRef = ctx.SafeRefProvider.GetOrCreateAssetSafeRef(objSafeRef.Component.sharedMesh);
            var rootBoneRef = ctx.SafeRefProvider.GetOrCreateComponentSafeRef(objSafeRef.Component.rootBone);

            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.Mesh = GetAssetIdentifierPayload(meshSafeRef);

            updateSample.RootBone = GetComponentIdentifierPayload(rootBoneRef);
            updateSample.Bones = new SkinnedMeshRendererUpdate.Types.Bones();

            if (objSafeRef.Component.bones != null)
            {
                foreach (var bone in objSafeRef.Component.bones)
                {
                    var boneRef = ctx.SafeRefProvider.GetOrCreateComponentSafeRef(bone);
                    updateSample.Bones.Ids.Add(GetComponentIdentifierPayload(boneRef));
                }
            }

            updateSample.BlendShapeWeights = new SkinnedMeshRendererUpdate.Types.BlendShapeWeights();

            if (objSafeRef.Component.sharedMesh != null)
            {
                for (var i = 0; i < objSafeRef.Component.sharedMesh.blendShapeCount; i++)
                {
                    updateSample.BlendShapeWeights.Weights.Add(
                        new SkinnedMeshRendererUpdate.Types.BlendShapeWeights.Types.BlendShapeWeight
                        {
                            Index = i,
                            Weight = objSafeRef.Component.GetBlendShapeWeight(i)
                        });
                }
            }

            _createSamples[objSafeRef] = new SkinnedMeshRendererCreate
                { Component = GetComponentIdentifierPayload(objSafeRef) };
        }

        protected override void OnObjectMarkedDestroyed(SkinnedMeshRendererSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedDestroyed(objSafeRef, ctx);

            _destroySamples[objSafeRef] = new SkinnedMeshRendererDestroy
                { Component = GetComponentIdentifierPayload(objSafeRef) };
        }

        private void OnBlendShapeWeightChanged(SkinnedMeshRenderer skinnedMeshRenderer, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.SafeRefProvider.GetOrCreateComponentSafeRef(skinnedMeshRenderer);

            if (!IsRecordingObject(objSafeRef))
                return;

            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.BlendShapeWeights = new SkinnedMeshRendererUpdate.Types.BlendShapeWeights();

            for (var i = 0; i < objSafeRef.Component.sharedMesh.blendShapeCount; i++)
            {
                updateSample.BlendShapeWeights.Weights.Add(
                    new SkinnedMeshRendererUpdate.Types.BlendShapeWeights.Types.BlendShapeWeight
                    {
                        Index = i,
                        Weight = objSafeRef.Component.GetBlendShapeWeight(i)
                    });
            }
        }

        private void OnRootBoneChanged(SkinnedMeshRenderer skinnedMeshRenderer, UnityEngine.Transform rootBone,
            RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.SafeRefProvider.GetOrCreateComponentSafeRef(skinnedMeshRenderer);

            if (!IsRecordingObject(objSafeRef))
                return;

            var rootBoneSafeRef = ctx.SafeRefProvider.GetOrCreateComponentSafeRef(rootBone);

            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.RootBone = GetComponentIdentifierPayload(rootBoneSafeRef);
        }

        private void OnBonesChanged(SkinnedMeshRenderer skinnedMeshRenderer,
            IEnumerable<UnityEngine.Transform> bones,
            RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.SafeRefProvider.GetOrCreateComponentSafeRef(skinnedMeshRenderer);

            if (!IsRecordingObject(objSafeRef))
                return;

            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.Bones = new SkinnedMeshRendererUpdate.Types.Bones();

            foreach (var bone in bones)
            {
                var boneSafeRef = ctx.SafeRefProvider.GetOrCreateComponentSafeRef(bone);
                updateSample.Bones.Ids.Add(GetComponentIdentifierPayload(boneSafeRef));
            }
        }

        private SkinnedMeshRendererUpdate GetOrCreateUpdateSample(SkinnedMeshRendererSafeRef objSafeRef)
        {
            if (_updateSamples.TryGetValue(objSafeRef, out var sample))
                return sample;
            sample = new SkinnedMeshRendererUpdate { Component = GetComponentIdentifierPayload(objSafeRef) };
            _updateSamples[objSafeRef] = sample;
            return sample;
        }

        protected override SkinnedMeshRendererFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            var frameData = SkinnedMeshRendererFrameData.Pool.Get();
            frameData.AddCreateSamples(_createSamples.Values);
            frameData.AddDestroySamples(_destroySamples.Values);
            frameData.AddUpdateSamples(_updateSamples.Values);
            frameData.AddUpdateSamples(GetRendererUpdateSamples());
            return frameData;
        }

        protected override void AfterCollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            base.AfterCollectFrameData(frameInfo, ctx);
            _updateSamples.Clear();
            _createSamples.Clear();
            _destroySamples.Clear();
        }
    }
}