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

        public static void SetTextAndNotify(Text text, string value)
        {
            var previousText = text.text;
            text.text = value;
            if (previousText != value)
            {
                OnTextChanged(text, value);
            }
        }
    }
}