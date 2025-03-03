using System.Collections.Generic;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.UI;
using UnityEngine.Scripting;
using ImageSafeRef = PLUME.Core.Object.SafeRef.IComponentSafeRef<UnityEngine.UI.Image>;
using static PLUME.Core.Utils.SampleUtils;

namespace PLUME.Base.Module.Unity.UI.Graphics.Image
{
    [Preserve]
    public class ImageRecorderModule : GraphicRecorderModule<UnityEngine.UI.Image, ImageFrameData>
    {
        private readonly Dictionary<ImageSafeRef, ImageCreate> _createSamples = new();
        private readonly Dictionary<ImageSafeRef, ImageDestroy> _destroySamples = new();
        private readonly Dictionary<ImageSafeRef, ImageUpdate> _updateSamples = new();
        
        protected override void OnObjectMarkedCreated(ImageSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);
            
            var image = objSafeRef.Component;
            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.Sprite = GetAssetIdentifierPayload(image.sprite);
            updateSample.Type = image.type.ToPayload();
            _createSamples[objSafeRef] = new ImageCreate { Component = GetComponentIdentifierPayload(objSafeRef) };
        }

        protected override void OnObjectMarkedDestroyed(ImageSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedDestroyed(objSafeRef, ctx);
            _destroySamples[objSafeRef] = new ImageDestroy { Component = GetComponentIdentifierPayload(objSafeRef) };
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