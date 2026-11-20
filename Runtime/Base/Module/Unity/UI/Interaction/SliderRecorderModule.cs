using PLUME.Base.Module.Unity.UI.Common;
using PLUME.Sample.Unity;
using PLUME.Sample.Unity.UI;
using UnityEngine.Scripting;
using Empty = Google.Protobuf.WellKnownTypes.Empty;

namespace PLUME.Base.Module.Unity.UI.Interaction
{
    [Preserve]
    public class SliderRecorderModule : PolledUIComponentRecorderModule<UnityEngine.UI.Slider,
        SliderRecorderModule.State, SliderCreate, SliderDestroy, SliderUpdate, Empty>
    {
        public struct State
        {
            public float Value;
            public float MinValue;
            public float MaxValue;
            public bool WholeNumbers;
            public int Direction;
        }

        protected override SliderCreate CreateCreateSample(ComponentIdentifier id) => new() { Component = id };

        protected override SliderDestroy CreateDestroySample(ComponentIdentifier id) => new() { Component = id };

        protected override SliderUpdate CreateUpdateSample(ComponentIdentifier id) => new() { Component = id };

        protected override State ReadState(UnityEngine.UI.Slider slider) => new()
        {
            Value = slider.value,
            MinValue = slider.minValue,
            MaxValue = slider.maxValue,
            WholeNumbers = slider.wholeNumbers,
            Direction = (int)slider.direction
        };

        protected override void WriteChanges(bool has, in State p, in State c, UpdateSampleBuilder sample)
        {
            if (UIDiff.Changed(has, p.Value, c.Value))
                sample.Get().Value = c.Value;
            if (UIDiff.Changed(has, p.MinValue, c.MinValue))
                sample.Get().MinValue = c.MinValue;
            if (UIDiff.Changed(has, p.MaxValue, c.MaxValue))
                sample.Get().MaxValue = c.MaxValue;
            if (UIDiff.Changed(has, p.WholeNumbers, c.WholeNumbers))
                sample.Get().WholeNumbers = c.WholeNumbers;
            if (UIDiff.Changed(has, p.Direction, c.Direction))
                sample.Get().Direction = (SliderDirection)c.Direction;
        }
    }
}
