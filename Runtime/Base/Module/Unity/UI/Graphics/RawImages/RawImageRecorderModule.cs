using System.Collections.Generic;
using PLUME.Base.Module.Unity.UI.Common;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.UI;
using UnityEngine;
using UnityEngine.Scripting;
using RawImageSafeRef = PLUME.Core.Object.SafeRef.IComponentSafeRef<UnityEngine.UI.RawImage>;
using RawImageFrameData = PLUME.Base.Module.Unity.UI.Common.UIComponentFrameData<PLUME.Sample.Unity.UI.RawImageCreate,
    PLUME.Sample.Unity.UI.RawImageDestroy, PLUME.Sample.Unity.UI.RawImageUpdate, PLUME.Sample.Unity.UI.GraphicUpdate>;
using static PLUME.Core.Utils.SampleUtils;

namespace PLUME.Base.Module.Unity.UI.Graphics.RawImages
{
    [Preserve]
    public class RawImageRecorderModule : GraphicRecorderModule<UnityEngine.UI.RawImage, RawImageFrameData>
    {
        private struct RawImageState
        {
            public Texture Texture;
            public Rect UVRect;
        }

        private readonly List<RawImageCreate> _createSamples = new();
        private readonly List<RawImageDestroy> _destroySamples = new();
        private readonly Dictionary<RawImageSafeRef, RawImageUpdate> _updateSamples = new();
        private readonly Dictionary<RawImageSafeRef, RawImageState> _states = new();

        protected override void OnObjectMarkedCreated(RawImageSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);
            _createSamples.Add(new RawImageCreate { Component = GetComponentIdentifierPayload(objSafeRef) });
            _states.Remove(objSafeRef);
            PollRawImage(objSafeRef);
        }

        protected override void OnObjectMarkedDestroyed(RawImageSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedDestroyed(objSafeRef, ctx);
            _destroySamples.Add(new RawImageDestroy { Component = GetComponentIdentifierPayload(objSafeRef) });
        }

        protected override void OnStopRecordingObject(RawImageSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnStopRecordingObject(objSafeRef, ctx);
            _states.Remove(objSafeRef);
        }

        protected override void OnStopRecording(RecorderContext ctx)
        {
            base.OnStopRecording(ctx);
            _states.Clear();
        }

        protected override void BeforeCollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            base.BeforeCollectFrameData(frameInfo, ctx);

            var recorded = RecordedComponents;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < recorded.Count; i++)
                PollRawImage(recorded[i]);
        }

        private void PollRawImage(RawImageSafeRef objSafeRef)
        {
            var rawImage = objSafeRef.Component;

            if (rawImage == null)
                return;

            var current = new RawImageState { Texture = rawImage.texture, UVRect = rawImage.uvRect };
            var has = _states.TryGetValue(objSafeRef, out var previous);

            // Render textures and runtime textures are not in the asset bundle; their identifier still tells which
            // texture was shown.
            if (UIDiff.Changed(has, previous.Texture, current.Texture))
                GetOrCreateUpdateSample(objSafeRef).Texture = GetAssetIdentifierPayload(current.Texture);
            if (UIDiff.Changed(has, previous.UVRect, current.UVRect))
                GetOrCreateUpdateSample(objSafeRef).UvRect = current.UVRect.ToPayload();

            _states[objSafeRef] = current;
        }

        private RawImageUpdate GetOrCreateUpdateSample(RawImageSafeRef objSafeRef)
        {
            if (_updateSamples.TryGetValue(objSafeRef, out var sample))
                return sample;
            sample = new RawImageUpdate { Component = GetComponentIdentifierPayload(objSafeRef) };
            _updateSamples[objSafeRef] = sample;
            return sample;
        }

        protected override RawImageFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            var frameData = RawImageFrameData.Pool.Get();
            frameData.AddCreateSamples(_createSamples);
            frameData.AddDestroySamples(_destroySamples);
            frameData.AddUpdateSamples(_updateSamples.Values);
            frameData.AddEventSamples(GetGraphicUpdateSamples());
            return frameData;
        }

        protected override void AfterCollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            base.AfterCollectFrameData(frameInfo, ctx);
            _createSamples.Clear();
            _destroySamples.Clear();
            _updateSamples.Clear();
        }
    }
}
