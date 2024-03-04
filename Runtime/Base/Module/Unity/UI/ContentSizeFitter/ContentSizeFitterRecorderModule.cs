using System.Collections.Generic;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.UI;
using UnityEngine.Scripting;
using ContentSizeFitterSafeRef = PLUME.Core.Object.SafeRef.IComponentSafeRef<UnityEngine.UI.ContentSizeFitter>;
using static PLUME.Core.Utils.SampleUtils;

namespace PLUME.Base.Module.Unity.UI.ContentSizeFitter
{
    [Preserve]
    public class ContentSizeFitterRecorderModule : ComponentRecorderModule<UnityEngine.UI.ContentSizeFitter, ContentSizeFitterFrameData>
    {
        private readonly Dictionary<ContentSizeFitterSafeRef, ContentSizeFitterCreate> _createSamples = new();
        private readonly Dictionary<ContentSizeFitterSafeRef, ContentSizeFitterDestroy> _destroySamples = new();
        private readonly Dictionary<ContentSizeFitterSafeRef, ContentSizeFitterUpdate> _updateSamples = new();

        protected override void OnObjectMarkedCreated(ContentSizeFitterSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);

            var contentSizeFitter = objSafeRef.Component;
            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.HorizontalFit = contentSizeFitter.horizontalFit.ToPayload();
            updateSample.VerticalFit = contentSizeFitter.verticalFit.ToPayload();
            _createSamples[objSafeRef] = new ContentSizeFitterCreate { Id = GetComponentIdentifierPayload(objSafeRef) };
        }

        protected override void OnObjectMarkedDestroyed(ContentSizeFitterSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedDestroyed(objSafeRef, ctx);
            _destroySamples[objSafeRef] = new ContentSizeFitterDestroy { Id = GetComponentIdentifierPayload(objSafeRef) };
        }

        private ContentSizeFitterUpdate GetOrCreateUpdateSample(ContentSizeFitterSafeRef objSafeRef)
        {
            if (_updateSamples.TryGetValue(objSafeRef, out var sample))
                return sample;
            sample = new ContentSizeFitterUpdate { Id = GetComponentIdentifierPayload(objSafeRef) };
            _updateSamples[objSafeRef] = sample;
            return sample;
        }

        protected override ContentSizeFitterFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            var frameData = ContentSizeFitterFrameData.Pool.Get();
            frameData.AddCreateSamples(_createSamples.Values);
            frameData.AddDestroySamples(_destroySamples.Values);
            frameData.AddUpdateSamples(_updateSamples.Values);
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