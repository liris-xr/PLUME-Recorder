using System;
using PLUME.Core;
using UnityEngine;
using UnityEngine.Scripting;

namespace PLUME.Base.Hooks
{
    public static class TransformHooks
    {
        public static Action<Transform, Transform> OnSetParent;

        public static Action<Transform, int> OnSetSiblingIndex;
        
        [Preserve]
        [RegisterHookAfterMethod(typeof(Transform), nameof(Transform.SetParent), typeof(Transform))]
        public static void SetParentHook(Transform transform, Transform parent)
        {
            OnSetParent?.Invoke(transform, parent);
        }
        
        [Preserve]
        [RegisterHookAfterPropertySetter(typeof(Transform), nameof(Transform.parent))]
        public static void ParentPropertySetterHook(Transform transform, Transform parent)
        {
            OnSetParent?.Invoke(transform, parent);
        }
        
        [Preserve]
        [RegisterHookAfterMethod(typeof(Transform), nameof(Transform.SetSiblingIndex), typeof(int))]
        public static void SetSiblingIndexHook(Transform transform, int siblingIdx)
        {
            OnSetSiblingIndex?.Invoke(transform, siblingIdx);
        }
        
        [Preserve]
        [RegisterHookAfterMethod(typeof(Transform), nameof(Transform.SetAsLastSibling))]
        public static void SetAsLastSiblingHook(Transform transform)
        {
            OnSetSiblingIndex?.Invoke(transform, transform.GetSiblingIndex());
        }
        
        [Preserve]
        [RegisterHookAfterMethod(typeof(Transform), nameof(Transform.SetAsFirstSibling))]
        public static void SetAsFirstSiblingHook(Transform transform)
        {
            OnSetSiblingIndex?.Invoke(transform, transform.GetSiblingIndex());
        }
    }
}