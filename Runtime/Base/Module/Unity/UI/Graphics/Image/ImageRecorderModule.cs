using System.Collections.Generic;
using PLUME.Base.Module.Unity.UI.Common;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.UI;
using UnityEngine;
using UnityEngine.Scripting;
using ImageSafeRef = PLUME.Core.Object.SafeRef.IComponentSafeRef<UnityEngine.UI.Image>;
using static PLUME.Core.Utils.SampleUtils;

namespace PLUME.Base.Module.Unity.UI.Graphics.Image
{
    [Preserve]
    public class ImageRecorderModule : GraphicRecorderModule<UnityEngine.UI.Image, ImageFrameData>
    {
        private struct ImageState
        {
            public Sprite Sprite;
            public UnityEngine.UI.Image.Type Type;
            public bool PreserveAspect;
            public bool FillCenter;
            public UnityEngine.UI.Image.FillMethod FillMethod;
            public float FillAmount;
            public bool FillClockwise;
            public int FillOrigin;
            public bool UseSpriteMesh;
            public float PixelsPerUnitMultiplier;
        }

        private readonly Dictionary<ImageSafeRef, ImageCreate> _createSamples = new();
        private readonly Dictionary<ImageSafeRef, ImageDestroy> _destroySamples = new();
        private readonly Dictionary<ImageSafeRef, ImageUpdate> _updateSamples = new();

        // Last recorded state per image, compared every frame (sprite swaps, fill animations, runtime styling...).
        private readonly Dictionary<ImageSafeRef, ImageState> _states = new();

        protected override void OnObjectMarkedCreated(ImageSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);
            _createSamples[objSafeRef] = new ImageCreate { Component = GetComponentIdentifierPayload(objSafeRef) };
            _states.Remove(objSafeRef);
            PollImage(objSafeRef);
        }

        protected override void OnObjectMarkedDestroyed(ImageSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedDestroyed(objSafeRef, ctx);
            _destroySamples[objSafeRef] = new ImageDestroy { Component = GetComponentIdentifierPayload(objSafeRef) };
        }

        protected override void OnStopRecordingObject(ImageSafeRef objSafeRef, RecorderContext ctx)
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
                PollImage(recorded[i]);
        }

        private void PollImage(ImageSafeRef objSafeRef)
        {
            var image = objSafeRef.Component;

            if (image == null)
                return;

            var current = new ImageState
            {
                Sprite = image.sprite,
                Type = image.type,
                PreserveAspect = image.preserveAspect,
                FillCenter = image.fillCenter,
                FillMethod = image.fillMethod,
                FillAmount = image.fillAmount,
                FillClockwise = image.fillClockwise,
                FillOrigin = image.fillOrigin,
                UseSpriteMesh = image.useSpriteMesh,
                PixelsPerUnitMultiplier = image.pixelsPerUnitMultiplier
            };

            var has = _states.TryGetValue(objSafeRef, out var previous);

            if (UIDiff.Changed(has, previous.Sprite, current.Sprite))
                GetOrCreateUpdateSample(objSafeRef).Sprite = GetAssetIdentifierPayload(current.Sprite);
            if (UIDiff.Changed(has, (int)previous.Type, (int)current.Type))
                GetOrCreateUpdateSample(objSafeRef).Type = current.Type.ToPayload();
            if (UIDiff.Changed(has, previous.PreserveAspect, current.PreserveAspect))
                GetOrCreateUpdateSample(objSafeRef).PreserveAspect = current.PreserveAspect;
            if (UIDiff.Changed(has, previous.FillCenter, current.FillCenter))
                GetOrCreateUpdateSample(objSafeRef).FillCenter = current.FillCenter;
            if (UIDiff.Changed(has, (int)previous.FillMethod, (int)current.FillMethod))
                GetOrCreateUpdateSample(objSafeRef).FillMethod = (ImageFillMethod)(int)current.FillMethod;
            if (UIDiff.Changed(has, previous.FillAmount, current.FillAmount))
                GetOrCreateUpdateSample(objSafeRef).FillAmount = current.FillAmount;
            if (UIDiff.Changed(has, previous.FillClockwise, current.FillClockwise))
                GetOrCreateUpdateSample(objSafeRef).FillClockwise = current.FillClockwise;
            if (UIDiff.Changed(has, previous.FillOrigin, current.FillOrigin))
                GetOrCreateUpdateSample(objSafeRef).FillOrigin = current.FillOrigin;
            if (UIDiff.Changed(has, previous.UseSpriteMesh, current.UseSpriteMesh))
                GetOrCreateUpdateSample(objSafeRef).UseSpriteMesh = current.UseSpriteMesh;
            if (UIDiff.Changed(has, previous.PixelsPerUnitMultiplier, current.PixelsPerUnitMultiplier))
                GetOrCreateUpdateSample(objSafeRef).PixelsPerUnitMultiplier = current.PixelsPerUnitMultiplier;

            _states[objSafeRef] = current;
        }

        private ImageUpdate GetOrCreateUpdateSample(ImageSafeRef objSafeRef)
        {
            if (_updateSamples.TryGetValue(objSafeRef, out var sample))
                return sample;
            sample = new ImageUpdate { Component = GetComponentIdentifierPayload(objSafeRef) };
            _updateSamples[objSafeRef] = sample;
            return sample;
        }

        protected override ImageFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            var frameData = ImageFrameData.Pool.Get();
            frameData.AddCreateSamples(_createSamples.Values);
            frameData.AddDestroySamples(_destroySamples.Values);
            frameData.AddUpdateSamples(_updateSamples.Values);
            frameData.AddGraphicUpdateSamples(GetGraphicUpdateSamples());
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
