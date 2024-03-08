using PLUME.Core;
using UnityEngine.Scripting;
using UnityEngine.UI;

namespace PLUME.Base.Events
{
    public static class TextEvents
    {
        public delegate void OnTextChangedDelegate(Text text, string value);
        
        public static event OnTextChangedDelegate OnTextChanged = delegate { };
        
        [Preserve]
        [RegisterPropertySetterDetour(typeof(Text), nameof(Text.text))]
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