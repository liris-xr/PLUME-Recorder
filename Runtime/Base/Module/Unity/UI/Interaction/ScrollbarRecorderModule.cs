using PLUME.Base.Module.Unity.UI.Common;
using PLUME.Sample.Unity;
using PLUME.Sample.Unity.UI;
using UnityEngine.Scripting;
using Empty = Google.Protobuf.WellKnownTypes.Empty;

namespace PLUME.Base.Module.Unity.UI.Interaction
{
    [Preserve]
    public class ScrollbarRecorderModule : PolledUIComponentRecorderModule<UnityEngine.UI.Scrollbar,
        ScrollbarRecorderModule.State, ScrollbarCreate, ScrollbarDestroy, ScrollbarUpdate, Empty>
    {
        public struct State
        {
            public float Value;
            public float Size;
            public int NumberOfSteps;
            public int Direction;
        }

        protected override ScrollbarCreate CreateCreateSample(ComponentIdentifier id) => new() { Component = id };

        protected override ScrollbarDestroy CreateDestroySample(ComponentIdentifier id) => new() { Component = id };

        protected override ScrollbarUpdate CreateUpdateSample(ComponentIdentifier id) => new() { Component = id };

        protected override State ReadState(UnityEngine.UI.Scrollbar scrollbar) => new()
        {
            Value = scrollbar.value,
            Size = scrollbar.size,
            NumberOfSteps = scrollbar.numberOfSteps,
            Direction = (int)scrollbar.direction
        };

        protected override void WriteChanges(bool has, in State p, in State c, UpdateSampleBuilder sample)
        {
            if (UIDiff.Changed(has, p.Value, c.Value))
                sample.Get().Value = c.Value;
            if (UIDiff.Changed(has, p.Size, c.Size))
                sample.Get().Size = c.Size;
            if (UIDiff.Changed(has, p.NumberOfSteps, c.NumberOfSteps))
                sample.Get().NumberOfSteps = c.NumberOfSteps;
            if (UIDiff.Changed(has, p.Direction, c.Direction))
                sample.Get().Direction = (ScrollbarDirection)c.Direction;
        }
    }
}
