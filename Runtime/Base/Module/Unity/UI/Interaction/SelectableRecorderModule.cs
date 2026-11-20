using System;
using System.Reflection;
using PLUME.Base.Module.Unity.UI.Common;
using PLUME.Sample.Unity;
using PLUME.Sample.Unity.UI;
using UnityEngine.EventSystems;
using UnityEngine.Scripting;
using Empty = Google.Protobuf.WellKnownTypes.Empty;
using static PLUME.Core.Utils.SampleUtils;

namespace PLUME.Base.Module.Unity.UI.Interaction
{
    /// <summary>
    /// Records the interaction state of every <see cref="UnityEngine.UI.Selectable"/> (Button, Toggle, Slider,
    /// Scrollbar, Dropdown, InputField, their TMP variants and custom selectables). The selection state tells when an
    /// element was hovered (highlighted), pressed, selected or disabled, whatever the input device.
    /// </summary>
    [Preserve]
    public class SelectableRecorderModule : PolledUIComponentRecorderModule<UnityEngine.UI.Selectable,
        SelectableRecorderModule.State, SelectableCreate, SelectableDestroy, SelectableUpdate, Empty>
    {
        public struct State
        {
            public bool Enabled;
            public bool Interactable;
            public int SelectionState;
            public int Transition;
            public UnityEngine.UI.Graphic TargetGraphic;
        }

        // Selectable.currentSelectionState is protected; it is the exact state driving the visual transition.
        private static readonly PropertyInfo CurrentSelectionStateProperty = typeof(UnityEngine.UI.Selectable)
            .GetProperty("currentSelectionState", BindingFlags.Instance | BindingFlags.NonPublic);

        protected override SelectableCreate CreateCreateSample(ComponentIdentifier id) => new() { Component = id };

        protected override SelectableDestroy CreateDestroySample(ComponentIdentifier id) => new() { Component = id };

        protected override SelectableUpdate CreateUpdateSample(ComponentIdentifier id) => new() { Component = id };

        protected override State ReadState(UnityEngine.UI.Selectable selectable) => new()
        {
            Enabled = selectable.enabled,
            Interactable = selectable.interactable,
            SelectionState = GetSelectionState(selectable),
            Transition = (int)selectable.transition,
            TargetGraphic = selectable.targetGraphic
        };

        protected override void WriteChanges(bool has, in State p, in State c, UpdateSampleBuilder sample)
        {
            if (UIDiff.Changed(has, p.Enabled, c.Enabled))
                sample.Get().Enabled = c.Enabled;
            if (UIDiff.Changed(has, p.Interactable, c.Interactable))
                sample.Get().Interactable = c.Interactable;
            if (UIDiff.Changed(has, p.SelectionState, c.SelectionState))
                sample.Get().SelectionState = (SelectionState)c.SelectionState;
            if (UIDiff.Changed(has, p.Transition, c.Transition))
                sample.Get().Transition = (SelectableTransition)c.Transition;
            if (UIDiff.Changed(has, p.TargetGraphic, c.TargetGraphic))
                sample.Get().TargetGraphic = GetComponentIdentifierPayload(c.TargetGraphic);
        }

        private static int GetSelectionState(UnityEngine.UI.Selectable selectable)
        {
            if (CurrentSelectionStateProperty != null)
            {
                try
                {
                    // Boxes an enum: a few bytes per selectable per frame.
                    return Convert.ToInt32(CurrentSelectionStateProperty.GetValue(selectable));
                }
                catch (Exception)
                {
                    // Fall through to the approximation below (e.g. property stripped from a player build).
                }
            }

            // Approximation from public state: hover and press are not observable without the protected state.
            if (!selectable.IsInteractable())
                return (int)SelectionState.Disabled;

            var eventSystem = EventSystem.current;
            if (eventSystem != null && eventSystem.currentSelectedGameObject == selectable.gameObject)
                return (int)SelectionState.Selected;

            return (int)SelectionState.Normal;
        }
    }
}
