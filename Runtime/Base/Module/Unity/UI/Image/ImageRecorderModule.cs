using System.Collections.Generic;
using PLUME.Base.Events;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.UI;
using UnityEngine.Scripting;
using ImageSafeRef = PLUME.Core.Object.SafeRef.IComponentSafeRef<UnityEngine.UI.Image>;
using static PLUME.Core.Utils.SampleUtils;

namespace PLUME.Base.Module.Unity.UI.Image
{
    [Preserve]
    public class ImageRecorderModule : ComponentRecorderModule<UnityEngine.UI.Image, ImageFrameData>
    {
        private readonly Dictionary<ImageSafeRef, ImageCreate> _createSamples = new();
        private readonly Dictionary<ImageSafeRef, ImageDestroy> _destroySamples = new();
        private readonly Dictionary<ImageSafeRef, ImageUpdate> _updateSamples = new();
        
        protected override void OnObjectMarkedCreated(ImageSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);
            
            var image = objSafeRef.Component;
            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.SpriteId = GetAssetIdentifierPayload(image.sprite);
            updateSample.MaterialId = GetAssetIdentifierPayload(image.material);
            updateSample.Color = image.color.ToPayload();
            _createSamples[objSafeRef] = new ImageCreate { Id = GetComponentIdentifierPayload(objSafeRef) };
        }

        protected override void OnObjectMarkedDestroyed(ImageSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedDestroyed(objSafeRef, ctx);
            _destroySamples[objSafeRef] = new ImageDestroy { Id = GetComponentIdentifierPayload(objSafeRef) };
        }

        private ImageUpdate GetOrCreateUpdateSample(ImageSafeRef objSafeRef)
        {
            if (_updateSamples.TryGetValue(objSafeRef, out var sample))
                return sample;
            sample = new ImageUpdate { Id = GetComponentIdentifierPayload(objSafeRef) };
            _updateSamples[objSafeRef] = sample;
            return sample;
        }

        protected override ImageFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            var frameData = ImageFrameData.Pool.Get();
            frameData.AddCreateSamples(_createSamples.Values);
            frameData.AddDestroySamples(_destroySamples.Values);
            frameData.AddUpdateSamples(_updateSamples.Values);
            return frameData;
        }

        protected override void OnAfterCollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            base.OnAfterCollectFrameData(frameInfo, ctx);
            _createSamples.Clear();
            _destroySamples.Clear();
            _updateSamples.Clear();
        }
    }
}