using System;
using PLUME.Core.Hooks;
using UnityEngine.Scripting;
using UnityEngine.UI;

namespace PLUME.Base.Hooks
{
    [Preserve]
    public class TextHooks : IRegisterHooksCallback
    {
        public delegate void OnTextChangedDelegate(Text text, string value);

        public static event OnTextChangedDelegate OnTextChanged = delegate { };

        public void RegisterHooks(HooksRegistry hooksRegistry)
        {
            hooksRegistry.RegisterHook(typeof(TextHooks).GetMethod(nameof(SetTextAndNotify)),
                typeof(Text).GetProperty(nameof(Text.text))!.GetSetMethod());
        }

        private static void NotifyTextChanged(Text text, string value)
        {
            try
            {
                OnTextChanged(text, value);
            }
            catch (Exception e)
            {
                HookErrors.Report($"{nameof(TextHooks)}.{nameof(OnTextChanged)}",
                    "Failed to record a text change. The application is not affected.", e);
            }
        }

        public static void SetTextAndNotify(Text text, string value)
        {
            // A null or destroyed Text is left to the user's call, so that it behaves as the original call.
            if (text == null)
            {
                text.text = value;
                return;
            }

            var previousText = text.text;
            text.text = value;
            if (previousText != value)
            {
                NotifyTextChanged(text, value);
            }
        }
    }
}