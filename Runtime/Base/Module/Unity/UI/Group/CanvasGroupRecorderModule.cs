using Google.Protobuf.WellKnownTypes;
using PLUME.Base.Module.Unity.UI.Common;
using PLUME.Sample.Unity;
using PLUME.Sample.Unity.UI;
using UnityEngine.Scripting;

namespace PLUME.Base.Module.Unity.UI.Group
{
    [Preserve]
    public class CanvasGroupRecorderModule : PolledUIComponentRecorderModule<UnityEngine.CanvasGroup,
        CanvasGroupRecorderModule.State, CanvasGroupCreate, CanvasGroupDestroy, CanvasGroupUpdate, Empty>
    {
        public struct State
        {
            public bool Enabled;
            public float Alpha;
            public bool Interactable;
            public bool BlocksRaycasts;
            public bool IgnoreParentGroups;
        }

        protected override CanvasGroupCreate CreateCreateSample(ComponentIdentifier id) => new() { Component = id };

        protected override CanvasGroupDestroy CreateDestroySample(ComponentIdentifier id) => new() { Component = id };

        protected override CanvasGroupUpdate CreateUpdateSample(ComponentIdentifier id) => new() { Component = id };

        protected override State ReadState(UnityEngine.CanvasGroup group) => new()
        {
            Enabled = group.enabled,
            Alpha = group.alpha,
            Interactable = group.interactable,
            BlocksRaycasts = group.blocksRaycasts,
            IgnoreParentGroups = group.ignoreParentGroups
        };

        protected override void WriteChanges(bool has, in State p, in State c, UpdateSampleBuilder sample)
        {
            if (UIDiff.Changed(has, p.Enabled, c.Enabled))
                sample.Get().Enabled = c.Enabled;
            if (UIDiff.Changed(has, p.Alpha, c.Alpha))
                sample.Get().Alpha = c.Alpha;
            if (UIDiff.Changed(has, p.Interactable, c.Interactable))
                sample.Get().Interactable = c.Interactable;
            if (UIDiff.Changed(has, p.BlocksRaycasts, c.BlocksRaycasts))
                sample.Get().BlocksRaycasts = c.BlocksRaycasts;
            if (UIDiff.Changed(has, p.IgnoreParentGroups, c.IgnoreParentGroups))
                sample.Get().IgnoreParentGroups = c.IgnoreParentGroups;
        }
    }
}
