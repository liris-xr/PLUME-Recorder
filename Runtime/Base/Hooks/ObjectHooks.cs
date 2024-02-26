using PLUME.Core;
using UnityEngine;

namespace PLUME.Base.Hooks
{
    public static class ObjectHooks
    {
        public static System.Action<Object, string> OnSetName;

        [RegisterPropertySetterHook(typeof(Object), nameof(Object.name))]
        public static void SetNameHook(Object obj, string name)
        {
            OnSetName?.Invoke(obj, name);
        }
    }
}