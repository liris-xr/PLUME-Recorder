using System.Collections.Generic;
using PLUME.Base.Hooks;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Core.Utils;
using PLUME.Sample.Unity;
using UnityEngine;
using UnityEngine.Scripting;
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

            SkinnedMeshRendererHooks.OnSetBones += (mr, bones) => OnSetBones(mr, bones, ctx);
        }

        protected override void OnObjectMarkedCreated(SkinnedMeshRendererSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);

            var meshSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateAssetSafeRef(objSafeRef.Component.sharedMesh);
            var rootBoneRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(objSafeRef.Component.rootBone);

            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.Bones = new SkinnedMeshRendererUpdate.Types.Bones();
            updateSample.MeshId = meshSafeRef.ToAssetIdentifierPayload();
            updateSample.Bones.RootBoneId = rootBoneRef.ToIdentifierPayload();

            foreach (var bone in objSafeRef.Component.bones)
            {
                var boneRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(bone);
                updateSample.Bones.BonesIds.Add(boneRef.ToIdentifierPayload());
            }

            _createSamples[objSafeRef] = new SkinnedMeshRendererCreate { Id = objSafeRef.ToIdentifierPayload() };
        }

        protected override void OnObjectMarkedDestroyed(SkinnedMeshRendererSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedDestroyed(objSafeRef, ctx);

            _destroySamples[objSafeRef] = new SkinnedMeshRendererDestroy { Id = objSafeRef.ToIdentifierPayload() };
        }

        private void OnSetBones(SkinnedMeshRenderer skinnedMeshRenderer, IEnumerable<UnityEngine.Transform> bones,
            RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(skinnedMeshRenderer);

            if (!IsRecordingObject(objSafeRef))
                return;

            var rootBoneSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(objSafeRef.Component.rootBone);

            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.Bones = new SkinnedMeshRendererUpdate.Types.Bones
            {
                RootBoneId = rootBoneSafeRef.ToIdentifierPayload()
            };

            foreach (var bone in bones)
            {
                var boneSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(bone);
                updateSample.Bones.BonesIds.Add(boneSafeRef.ToIdentifierPayload());
            }
        }

        private SkinnedMeshRendererUpdate GetOrCreateUpdateSample(SkinnedMeshRendererSafeRef objSafeRef)
        {
            if (_updateSamples.TryGetValue(objSafeRef, out var sample))
                return sample;
            sample = new SkinnedMeshRendererUpdate { Id = objSafeRef.ToIdentifierPayload() };
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

        protected override void OnAfterCollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            base.OnAfterCollectFrameData(frameInfo, ctx);
            _updateSamples.Clear();
            _createSamples.Clear();
            _destroySamples.Clear();
        }
    }
}