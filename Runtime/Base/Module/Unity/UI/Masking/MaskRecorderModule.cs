using Google.Protobuf.WellKnownTypes;
using PLUME.Base.Module.Unity.UI.Common;
using PLUME.Sample.Unity;
using PLUME.Sample.Unity.UI;
using UnityEngine.Scripting;

namespace PLUME.Base.Module.Unity.UI.Masking
{
    [Preserve]
    public class MaskRecorderModule : PolledUIComponentRecorderModule<UnityEngine.UI.Mask,
        MaskRecorderModule.State, MaskCreate, MaskDestroy, MaskUpdate, Empty>
    {
        public struct State
        {
            public bool Enabled;
            public bool ShowMaskGraphic;
        }

        protected override MaskCreate CreateCreateSample(ComponentIdentifier id) => new() { Component = id };

        protected override MaskDestroy CreateDestroySample(ComponentIdentifier id) => new() { Component = id };

        protected override MaskUpdate CreateUpdateSample(ComponentIdentifier id) => new() { Component = id };

        protected override State ReadState(UnityEngine.UI.Mask mask) => new()
        {
            Enabled = mask.enabled,
            ShowMaskGraphic = mask.showMaskGraphic
        };

        protected override void WriteChanges(bool has, in State p, in State c, UpdateSampleBuilder sample)
        {
            if (UIDiff.Changed(has, p.Enabled, c.Enabled))
                sample.Get().Enabled = c.Enabled;
            if (UIDiff.Changed(has, p.ShowMaskGraphic, c.ShowMaskGraphic))
                sample.Get().ShowMaskGraphic = c.ShowMaskGraphic;
        }
    }
}
