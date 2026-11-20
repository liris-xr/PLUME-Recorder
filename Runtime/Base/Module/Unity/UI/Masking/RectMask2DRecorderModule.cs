using Google.Protobuf.WellKnownTypes;
using PLUME.Base.Module.Unity.UI.Common;
using PLUME.Sample.Unity;
using PLUME.Sample.Unity.UI;
using UnityEngine;
using UnityEngine.Scripting;
using static PLUME.Core.Utils.SampleUtils;

namespace PLUME.Base.Module.Unity.UI.Masking
{
    [Preserve]
    public class RectMask2DRecorderModule : PolledUIComponentRecorderModule<UnityEngine.UI.RectMask2D,
        RectMask2DRecorderModule.State, RectMask2DCreate, RectMask2DDestroy, RectMask2DUpdate, Empty>
    {
        public struct State
        {
            public bool Enabled;
            public Vector4 Padding;
            public Vector2 Softness;
        }

        protected override RectMask2DCreate CreateCreateSample(ComponentIdentifier id) => new() { Component = id };

        protected override RectMask2DDestroy CreateDestroySample(ComponentIdentifier id) => new() { Component = id };

        protected override RectMask2DUpdate CreateUpdateSample(ComponentIdentifier id) => new() { Component = id };

        protected override State ReadState(UnityEngine.UI.RectMask2D mask) => new()
        {
            Enabled = mask.enabled,
            Padding = mask.padding,
            Softness = mask.softness
        };

        protected override void WriteChanges(bool has, in State p, in State c, UpdateSampleBuilder sample)
        {
            if (UIDiff.Changed(has, p.Enabled, c.Enabled))
                sample.Get().Enabled = c.Enabled;
            if (UIDiff.Changed(has, p.Padding, c.Padding))
                sample.Get().Padding = c.Padding.ToPayload();
            if (UIDiff.Changed(has, p.Softness, c.Softness))
                sample.Get().Softness = c.Softness.ToPayload();
        }
    }
}
