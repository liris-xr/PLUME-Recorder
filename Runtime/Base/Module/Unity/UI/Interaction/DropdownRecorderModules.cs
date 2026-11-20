using System.Collections.Generic;
using PLUME.Base.Module.Unity.UI.Common;
using PLUME.Sample.Unity;
using PLUME.Sample.Unity.UI;
using UnityEngine;
using UnityEngine.Scripting;
using Empty = Google.Protobuf.WellKnownTypes.Empty;

namespace PLUME.Base.Module.Unity.UI.Interaction
{
    /// <summary>
    /// Shared recording of the selected value and option labels of a dropdown. Option labels are hashed every frame
    /// (no allocation) and only sent when they change.
    /// </summary>
    public abstract class DropdownRecorderModuleBase<TC> : PolledUIComponentRecorderModule<TC,
        DropdownRecorderModuleBase<TC>.State, DropdownCreate, DropdownDestroy, DropdownUpdate, Empty>
        where TC : Component
    {
        public struct State
        {
            public int Value;
            public int OptionsHash;
            public TC Component;
        }

        protected abstract int GetValue(TC dropdown);

        protected abstract int GetOptionCount(TC dropdown);

        protected abstract string GetOptionText(TC dropdown, int index);

        protected override DropdownCreate CreateCreateSample(ComponentIdentifier id) => new() { Component = id };

        protected override DropdownDestroy CreateDestroySample(ComponentIdentifier id) => new() { Component = id };

        protected override DropdownUpdate CreateUpdateSample(ComponentIdentifier id) => new() { Component = id };

        protected override State ReadState(TC dropdown)
        {
            var hash = 17;
            var count = GetOptionCount(dropdown);

            for (var i = 0; i < count; i++)
                hash = hash * 31 + (GetOptionText(dropdown, i)?.GetHashCode() ?? 0);

            return new State
            {
                Value = GetValue(dropdown),
                OptionsHash = hash * 31 + count,
                Component = dropdown
            };
        }

        protected override void WriteChanges(bool has, in State p, in State c, UpdateSampleBuilder sample)
        {
            if (UIDiff.Changed(has, p.Value, c.Value))
                sample.Get().Value = c.Value;

            if (UIDiff.Changed(has, p.OptionsHash, c.OptionsHash))
            {
                var options = new DropdownUpdate.Types.DropdownOptions();
                var count = GetOptionCount(c.Component);

                for (var i = 0; i < count; i++)
                    options.Texts.Add(GetOptionText(c.Component, i) ?? "");

                sample.Get().Options = options;
            }
        }
    }

    [Preserve]
    public class DropdownRecorderModule : DropdownRecorderModuleBase<UnityEngine.UI.Dropdown>
    {
        protected override int GetValue(UnityEngine.UI.Dropdown dropdown) => dropdown.value;

        protected override int GetOptionCount(UnityEngine.UI.Dropdown dropdown) => dropdown.options.Count;

        protected override string GetOptionText(UnityEngine.UI.Dropdown dropdown, int index) =>
            dropdown.options[index].text;
    }

#if TMP_ENABLED
    [Preserve]
    public class TMPDropdownRecorderModule : DropdownRecorderModuleBase<TMPro.TMP_Dropdown>
    {
        protected override int GetValue(TMPro.TMP_Dropdown dropdown) => dropdown.value;

        protected override int GetOptionCount(TMPro.TMP_Dropdown dropdown) => dropdown.options.Count;

        protected override string GetOptionText(TMPro.TMP_Dropdown dropdown, int index) =>
            dropdown.options[index].text;
    }
#endif
}
