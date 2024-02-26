using System;
using PLUME.Core;
using UnityEngine;

namespace PLUME.Base.Hooks
{
    public static class TransformHooks
    {
        public static Action<Transform, Transform> OnSetParent;
        
        [RegisterMethodHook(typeof(Transform), nameof(Transform.SetParent), typeof(Transform))]
        public static void SetParentHook(Transform transform, Transform parent)
        {
            OnSetParent?.Invoke(transform, parent);
        }
        
        [RegisterPropertySetterHook(typeof(Transform), nameof(Transform.parent))]
        public static void ParentPropertySetterHook(Transform transform, Transform parent)
        {
            OnSetParent?.Invoke(transform, parent);
        }
    }
}