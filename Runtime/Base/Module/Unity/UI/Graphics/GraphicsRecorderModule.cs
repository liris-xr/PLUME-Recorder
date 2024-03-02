using System.Collections.Generic;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.UI;
using UnityEngine.UI;
using static PLUME.Core.Utils.SampleUtils;

namespace PLUME.Base.Module.Unity.UI.Graphics
{
    public abstract class GraphicRecorderModule<TG, TF> : ComponentRecorderModule<TG, TF>
        where TG : Graphic
        where TF : IFrameData
    {
        // Update samples for the current frame and for each component, entries are only added when a property changes
        private readonly Dictionary<IComponentSafeRef<TG>, GraphicUpdate> _updateSamples = new();
        
        protected override void OnObjectMarkedCreated(IComponentSafeRef<TG> objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);

            var graphic = objSafeRef.Component;
            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.MaterialId = GetAssetIdentifierPayload(graphic.material);
            updateSample.Color = graphic.color.ToPayload();
        }

        protected IEnumerable<GraphicUpdate> GetGraphicUpdateSamples()
        {
            return _updateSamples.Values;
        }

        protected override void OnAfterCollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            base.OnAfterCollectFrameData(frameInfo, ctx);
            _updateSamples.Clear();
        }

        private GraphicUpdate GetOrCreateUpdateSample(IComponentSafeRef<TG> objSafeRef)
        {
            if (_updateSamples.TryGetValue(objSafeRef, out var sample))
                return sample;
            _updateSamples[objSafeRef] = new GraphicUpdate { Id = GetComponentIdentifierPayload(objSafeRef) };
            return _updateSamples[objSafeRef];
        }
    }
}