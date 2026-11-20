using PLUME.Base.Module.Unity.UI.Common;
using PLUME.Sample.Unity;
using PLUME.Sample.Unity.UI;
using UnityEngine.Scripting;
using Empty = Google.Protobuf.WellKnownTypes.Empty;
using static PLUME.Core.Utils.SampleUtils;

namespace PLUME.Base.Module.Unity.UI.Interaction
{
    [Preserve]
    public class ToggleRecorderModule : PolledUIComponentRecorderModule<UnityEngine.UI.Toggle,
        ToggleRecorderModule.State, ToggleCreate, ToggleDestroy, ToggleUpdate, Empty>
    {
        public struct State
        {
            public bool IsOn;
            public UnityEngine.UI.ToggleGroup Group;
        }

        protected override ToggleCreate CreateCreateSample(ComponentIdentifier id) => new() { Component = id };

        protected override ToggleDestroy CreateDestroySample(ComponentIdentifier id) => new() { Component = id };

        protected override ToggleUpdate CreateUpdateSample(ComponentIdentifier id) => new() { Component = id };

        protected override State ReadState(UnityEngine.UI.Toggle toggle) => new()
        {
            IsOn = toggle.isOn,
            Group = toggle.group
        };

        protected override void WriteChanges(bool has, in State p, in State c, UpdateSampleBuilder sample)
        {
            if (UIDiff.Changed(has, p.IsOn, c.IsOn))
                sample.Get().IsOn = c.IsOn;
            if (UIDiff.Changed(has, p.Group, c.Group))
                sample.Get().Group = GetComponentIdentifierPayload(c.Group);
        }
    }
}
