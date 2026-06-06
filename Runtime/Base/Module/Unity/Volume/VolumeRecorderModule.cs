#if CORE_RP_ENABLED
using System.Collections.Generic;
using System.Linq;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.URP;
using UnityEngine.Scripting;
using static PLUME.Core.Utils.SampleUtils;
using VolumeSafeRef = PLUME.Core.Object.SafeRef.IComponentSafeRef<UnityEngine.Rendering.Volume>;

namespace PLUME.Base.Module.Unity.Volume
{
    [Preserve]
    public class VolumeRecorderModule : ComponentRecorderModule<UnityEngine.Rendering.Volume, VolumeFrameData>
    {
        private readonly Dictionary<VolumeSafeRef, VolumeCreate> _createSamples = new();
        private readonly Dictionary<VolumeSafeRef, VolumeDestroy> _destroySamples = new();
        private readonly Dictionary<VolumeSafeRef, VolumeUpdate> _updateSamples = new();
        private readonly Dictionary<VolumeSafeRef, VolumeUpdateEnabled> _updateEnabledSamples = new();

        protected override void OnCreate(RecorderContext ctx)
        {
            base.OnCreate(ctx);
            // TODO: add hooks
        }

        protected override void OnObjectMarkedCreated(VolumeSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);

            var volume = objSafeRef.Component;
            var sharedProfile = ctx.SafeRefProvider.GetOrCreateAssetSafeRef(volume.sharedProfile);

            var colliders = new VolumeUpdate.Types.Colliders();
            colliders.Ids.AddRange(volume.colliders.Select(GetComponentIdentifierPayload));

            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.IsGlobal = volume.isGlobal;
            updateSample.Colliders = colliders;
            updateSample.BlendDistance = volume.blendDistance;
            updateSample.Weight = volume.weight;
            updateSample.Priority = volume.priority;
            updateSample.SharedProfile = GetAssetIdentifierPayload(sharedProfile);

            var updateEnabledSample = GetOrCreateUpdateEnabledSample(objSafeRef);
            updateEnabledSample.Enabled = volume.enabled;

            _createSamples[objSafeRef] = new VolumeCreate
            {
                Component = GetComponentIdentifierPayload(objSafeRef)
            };
        }

        protected override void OnObjectMarkedDestroyed(VolumeSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedDestroyed(objSafeRef, ctx);
            _destroySamples[objSafeRef] = new VolumeDestroy { Component = GetComponentIdentifierPayload(objSafeRef) };
        }

        private VolumeUpdate GetOrCreateUpdateSample(VolumeSafeRef objSafeRef)
        {
            if (_updateSamples.TryGetValue(objSafeRef, out var sample))
                return sample;
            sample = new VolumeUpdate { Component = GetComponentIdentifierPayload(objSafeRef) };
            _updateSamples[objSafeRef] = sample;
            return sample;
        }

        private VolumeUpdateEnabled GetOrCreateUpdateEnabledSample(VolumeSafeRef objSafeRef)
        {
            if (_updateEnabledSamples.TryGetValue(objSafeRef, out var sample))
                return sample;
            sample = new VolumeUpdateEnabled { Component = GetComponentIdentifierPayload(objSafeRef) };
            _updateEnabledSamples[objSafeRef] = sample;
            return sample;
        }

        protected override VolumeFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            var frameData = VolumeFrameData.Pool.Get();
            frameData.AddCreateSamples(_createSamples.Values);
            frameData.AddDestroySamples(_destroySamples.Values);
            frameData.AddUpdateSamples(_updateSamples.Values);
            frameData.AddUpdateEnabledSamples(_updateEnabledSamples.Values);
            return frameData;
        }

        protected override void AfterCollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            base.AfterCollectFrameData(frameInfo, ctx);
            _createSamples.Clear();
            _destroySamples.Clear();
            _updateSamples.Clear();
            _updateEnabledSamples.Clear();
        }
    }
}
#endif
