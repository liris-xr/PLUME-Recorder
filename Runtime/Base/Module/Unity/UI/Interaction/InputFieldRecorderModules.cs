using System.Collections.Generic;
using PLUME.Base.Module.Unity.UI.Common;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder;
using PLUME.Sample.Unity;
using PLUME.Sample.Unity.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Scripting;
using static PLUME.Core.Utils.SampleUtils;

namespace PLUME.Base.Module.Unity.UI.Interaction
{
    /// <summary>
    /// Shared recording of an input field's text and focus, plus an <see cref="InputFieldEndEdit"/> event each time
    /// editing ends (submit or focus loss).
    /// </summary>
    public abstract class InputFieldRecorderModuleBase<TC> : PolledUIComponentRecorderModule<TC,
        InputFieldRecorderModuleBase<TC>.State, InputFieldCreate, InputFieldDestroy, InputFieldUpdate,
        InputFieldEndEdit>
        where TC : Component
    {
        public struct State
        {
            public string Text;
            public bool IsFocused;
        }

        private readonly Dictionary<IComponentSafeRef<TC>, UnityAction<string>> _listeners = new();

        protected abstract string GetText(TC inputField);

        protected abstract bool IsFocused(TC inputField);

        protected abstract UnityEvent<string> GetOnEndEdit(TC inputField);

        protected override InputFieldCreate CreateCreateSample(ComponentIdentifier id) => new() { Component = id };

        protected override InputFieldDestroy CreateDestroySample(ComponentIdentifier id) => new() { Component = id };

        protected override InputFieldUpdate CreateUpdateSample(ComponentIdentifier id) => new() { Component = id };

        protected override State ReadState(TC inputField) => new()
        {
            Text = GetText(inputField),
            IsFocused = IsFocused(inputField)
        };

        protected override void WriteChanges(bool has, in State p, in State c, UpdateSampleBuilder sample)
        {
            if (UIDiff.Changed(has, p.Text, c.Text))
                sample.Get().Text = c.Text ?? "";
            if (UIDiff.Changed(has, p.IsFocused, c.IsFocused))
                sample.Get().IsFocused = c.IsFocused;
        }

        protected override void OnStartRecordingObject(IComponentSafeRef<TC> objSafeRef, RecorderContext ctx)
        {
            base.OnStartRecordingObject(objSafeRef, ctx);

            var inputField = objSafeRef.Component;

            if (inputField == null || _listeners.ContainsKey(objSafeRef))
                return;

            void OnEndEdit(string text)
            {
                if (ctx.IsRecording)
                    AddEventSample(new InputFieldEndEdit
                    {
                        Component = GetComponentIdentifierPayload(objSafeRef),
                        Text = text ?? ""
                    });
            }

            UnityAction<string> listener = OnEndEdit;
            GetOnEndEdit(inputField).AddListener(listener);
            _listeners[objSafeRef] = listener;
        }

        protected override void OnStopRecordingObject(IComponentSafeRef<TC> objSafeRef, RecorderContext ctx)
        {
            base.OnStopRecordingObject(objSafeRef, ctx);

            if (!_listeners.Remove(objSafeRef, out var listener))
                return;

            var inputField = objSafeRef.Component;

            if (inputField != null)
                GetOnEndEdit(inputField).RemoveListener(listener);
        }

        protected override void OnStopRecording(RecorderContext ctx)
        {
            foreach (var (objSafeRef, listener) in _listeners)
            {
                var inputField = objSafeRef.Component;

                if (inputField != null)
                    GetOnEndEdit(inputField).RemoveListener(listener);
            }

            _listeners.Clear();
            base.OnStopRecording(ctx);
        }
    }

    [Preserve]
    public class InputFieldRecorderModule : InputFieldRecorderModuleBase<UnityEngine.UI.InputField>
    {
        protected override string GetText(UnityEngine.UI.InputField inputField) => inputField.text;

        protected override bool IsFocused(UnityEngine.UI.InputField inputField) => inputField.isFocused;

        protected override UnityEvent<string> GetOnEndEdit(UnityEngine.UI.InputField inputField) =>
            inputField.onEndEdit;
    }

#if TMP_ENABLED
    [Preserve]
    public class TMPInputFieldRecorderModule : InputFieldRecorderModuleBase<TMPro.TMP_InputField>
    {
        protected override string GetText(TMPro.TMP_InputField inputField) => inputField.text;

        protected override bool IsFocused(TMPro.TMP_InputField inputField) => inputField.isFocused;

        protected override UnityEvent<string> GetOnEndEdit(TMPro.TMP_InputField inputField) =>
            inputField.onEndEdit;
    }
#endif
}
