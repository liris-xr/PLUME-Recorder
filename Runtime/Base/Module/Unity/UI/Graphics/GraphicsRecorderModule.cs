using System.Collections.Generic;
using PLUME.Base.Module.Unity.UI.Common;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.UI;
using UnityEngine;
using UnityEngine.UI;
using static PLUME.Core.Utils.SampleUtils;

namespace PLUME.Base.Module.Unity.UI.Graphics
{
    public abstract class GraphicRecorderModule<TG, TF> : ComponentRecorderModule<TG, TF>
        where TG : Graphic
        where TF : IFrameData
    {
        private struct GraphicState
        {
            public bool Enabled;
            public Color Color;
            public Material Material;
            public bool RaycastTarget;
            public Vector4 RaycastPadding;
        }

        // Update samples for the current frame and for each component, entries are only added when a property changes
        private readonly Dictionary<IComponentSafeRef<TG>, GraphicUpdate> _updateSamples = new();

        // Last recorded state per graphic, compared every frame so that changes made after creation (script-driven
        // colors, enabled toggles, animated properties...) are recorded too.
        private readonly Dictionary<IComponentSafeRef<TG>, GraphicState> _graphicStates = new();

        /// <summary>Material identifying the graphic's look. Overridden where the <c>material</c> getter would
        /// instantiate a material (e.g. TextMeshPro).</summary>
        protected virtual Material GetRecordedMaterial(TG graphic) => graphic.material;

        protected override void OnObjectMarkedCreated(IComponentSafeRef<TG> objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);
            _graphicStates.Remove(objSafeRef);
            PollGraphic(objSafeRef);
        }

        protected override void OnStopRecordingObject(IComponentSafeRef<TG> objSafeRef, RecorderContext ctx)
        {
            base.OnStopRecordingObject(objSafeRef, ctx);
            _graphicStates.Remove(objSafeRef);
        }

        protected override void OnStopRecording(RecorderContext ctx)
        {
            base.OnStopRecording(ctx);
            _graphicStates.Clear();
            _updateSamples.Clear();
        }

        protected override void BeforeCollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            base.BeforeCollectFrameData(frameInfo, ctx);

            var recorded = RecordedComponents;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < recorded.Count; i++)
                PollGraphic(recorded[i]);
        }

        protected IEnumerable<GraphicUpdate> GetGraphicUpdateSamples()
        {
            return _updateSamples.Values;
        }

        protected override void AfterCollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            base.AfterCollectFrameData(frameInfo, ctx);
            _updateSamples.Clear();
        }

        private void PollGraphic(IComponentSafeRef<TG> objSafeRef)
        {
            var graphic = objSafeRef.Component;

            // Destroyed but not yet removed from the recorded objects.
            if (graphic == null)
                return;

            var current = new GraphicState
            {
                Enabled = graphic.enabled,
                Color = graphic.color,
                Material = GetRecordedMaterial(graphic),
                RaycastTarget = graphic.raycastTarget,
                RaycastPadding = graphic.raycastPadding
            };

            var has = _graphicStates.TryGetValue(objSafeRef, out var previous);

            if (UIDiff.Changed(has, previous.Enabled, current.Enabled))
                GetOrCreateUpdateSample(objSafeRef).Enabled = current.Enabled;
            if (UIDiff.Changed(has, previous.Color, current.Color))
                GetOrCreateUpdateSample(objSafeRef).Color = current.Color.ToPayload();
            if (UIDiff.Changed(has, previous.Material, current.Material))
                GetOrCreateUpdateSample(objSafeRef).Material = GetAssetIdentifierPayload(current.Material);
            if (UIDiff.Changed(has, previous.RaycastTarget, current.RaycastTarget))
                GetOrCreateUpdateSample(objSafeRef).RaycastTarget = current.RaycastTarget;
            if (UIDiff.Changed(has, previous.RaycastPadding, current.RaycastPadding))
                GetOrCreateUpdateSample(objSafeRef).RaycastPadding = current.RaycastPadding.ToPayload();

            _graphicStates[objSafeRef] = current;
        }

        private GraphicUpdate GetOrCreateUpdateSample(IComponentSafeRef<TG> objSafeRef)
        {
            if (_updateSamples.TryGetValue(objSafeRef, out var sample))
                return sample;
            _updateSamples[objSafeRef] = new GraphicUpdate { Component = GetComponentIdentifierPayload(objSafeRef) };
            return _updateSamples[objSafeRef];
        }
    }
}
