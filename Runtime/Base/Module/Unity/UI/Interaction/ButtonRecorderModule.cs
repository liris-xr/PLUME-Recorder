using System.Collections.Generic;
using PLUME.Base.Module.Unity.UI.Common;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder;
using PLUME.Sample.Unity;
using PLUME.Sample.Unity.UI;
using UnityEngine.Events;
using UnityEngine.Scripting;
using Empty = Google.Protobuf.WellKnownTypes.Empty;
using static PLUME.Core.Utils.SampleUtils;

namespace PLUME.Base.Module.Unity.UI.Interaction
{
    /// <summary>
    /// Records a <see cref="ButtonClick"/> each time a button's onClick is invoked, whatever triggered it (mouse, XR
    /// ray, poke, keyboard or gamepad submit). The button's lifecycle and visual state are recorded by
    /// <see cref="SelectableRecorderModule"/>.
    /// </summary>
    [Preserve]
    public class ButtonRecorderModule : PolledUIComponentRecorderModule<UnityEngine.UI.Button,
        ButtonRecorderModule.State, Empty, Empty, Empty, ButtonClick>
    {
        public struct State
        {
        }

        private readonly Dictionary<IComponentSafeRef<UnityEngine.UI.Button>, UnityAction> _listeners = new();

        protected override bool RecordLifecycle => false;

        protected override Empty CreateCreateSample(ComponentIdentifier id) => new();

        protected override Empty CreateDestroySample(ComponentIdentifier id) => new();

        protected override Empty CreateUpdateSample(ComponentIdentifier id) => new();

        protected override State ReadState(UnityEngine.UI.Button button) => default;

        protected override void WriteChanges(bool has, in State p, in State c, UpdateSampleBuilder sample)
        {
        }

        protected override void OnStartRecordingObject(IComponentSafeRef<UnityEngine.UI.Button> objSafeRef,
            RecorderContext ctx)
        {
            base.OnStartRecordingObject(objSafeRef, ctx);

            var button = objSafeRef.Component;

            if (button == null || _listeners.ContainsKey(objSafeRef))
                return;

            void OnClick()
            {
                if (ctx.IsRecording)
                    AddEventSample(new ButtonClick { Component = GetComponentIdentifierPayload(objSafeRef) });
            }

            UnityAction listener = OnClick;
            button.onClick.AddListener(listener);
            _listeners[objSafeRef] = listener;
        }

        protected override void OnStopRecordingObject(IComponentSafeRef<UnityEngine.UI.Button> objSafeRef,
            RecorderContext ctx)
        {
            base.OnStopRecordingObject(objSafeRef, ctx);

            if (!_listeners.Remove(objSafeRef, out var listener))
                return;

            var button = objSafeRef.Component;

            if (button != null)
                button.onClick.RemoveListener(listener);
        }

        protected override void OnStopRecording(RecorderContext ctx)
        {
            foreach (var (objSafeRef, listener) in _listeners)
            {
                var button = objSafeRef.Component;

                if (button != null)
                    button.onClick.RemoveListener(listener);
            }

            _listeners.Clear();
            base.OnStopRecording(ctx);
        }
    }
}
