using System;
using PLUME.Core;
using UnityEngine;

namespace PLUME.Base.Hooks
{
    public static class TransformHooks
    {
        public static Action<Transform, Transform> OnSetParent;

        public static Action<Transform, int> OnSetSiblingIndex;
        
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
        
        [RegisterMethodHook(typeof(Transform), nameof(Transform.SetSiblingIndex), typeof(int))]
        public static void SetSiblingIndexHook(Transform transform, int siblingIdx)
        {
            OnSetSiblingIndex?.Invoke(transform, siblingIdx);
        }
        
        [RegisterMethodHook(typeof(Transform), nameof(Transform.SetAsLastSibling))]
        public static void SetAsLastSiblingHook(Transform transform)
        {
            OnSetSiblingIndex?.Invoke(transform, transform.GetSiblingIndex());
        }
        
        [RegisterMethodHook(typeof(Transform), nameof(Transform.SetAsFirstSibling))]
        public static void SetAsFirstSiblingHook(Transform transform)
        {
            OnSetSiblingIndex?.Invoke(transform, transform.GetSiblingIndex());
        }
    }
}