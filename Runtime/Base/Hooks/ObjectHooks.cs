using PLUME.Core;
using UnityEngine;

namespace PLUME.Base.Hooks
{
    public static class ObjectHooks
    {
        public static System.Action<Object, string> OnSetName;

        public static System.Action<Object> OnBeforeDestroy;

        [RegisterHookAfterPropertySetter(typeof(Object), nameof(Object.name))]
        public static void SetNameHook(Object obj, string name)
        {
            OnSetName?.Invoke(obj, name);
        }

        [RegisterHookBeforeMethod(typeof(Object), nameof(Object.Destroy), typeof(Object))]
        public static void BeforeDestroyHook(Object obj)
        {
            if (obj != null)
                OnBeforeDestroy?.Invoke(obj);
        }

        [RegisterHookBeforeMethod(typeof(Object), nameof(Object.DestroyImmediate), typeof(Object))]
        public static void BeforeDestroyImmediateHook(Object obj)
        {
            if (obj != null)
                OnBeforeDestroy?.Invoke(obj);
        }
    }
}