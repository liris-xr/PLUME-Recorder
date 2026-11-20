using System;
using PLUME.Core.Hooks;
using UnityEngine;
using UnityEngine.Scripting;

namespace PLUME.Base.Hooks
{
    [Preserve]
    public class TransformHooks : IRegisterHooksCallback
    {
        public delegate void OnParentChangedDelegate(Transform t, Transform parent);

        public delegate void OnSiblingIndexChangedDelegate(Transform t, int siblingIdx);

        public static event OnParentChangedDelegate OnParentChanged = delegate { };

        public static event OnSiblingIndexChangedDelegate OnSiblingIndexChanged = delegate { };

        public void RegisterHooks(HooksRegistry hooksRegistry)
        {
            hooksRegistry.RegisterHook(typeof(TransformHooks).GetMethod(nameof(SetParentAndNotify), new[] {typeof(Transform), typeof(Transform)}),
                typeof(Transform).GetMethod(nameof(Transform.SetParent), new[] {typeof(Transform)}));
            hooksRegistry.RegisterHook(typeof(TransformHooks).GetMethod(nameof(SetParentAndNotify), new[] {typeof(Transform), typeof(Transform), typeof(bool)}),
                typeof(Transform).GetMethod(nameof(Transform.SetParent), new[] {typeof(Transform), typeof(bool)}));
            hooksRegistry.RegisterHook(typeof(TransformHooks).GetMethod(nameof(SetParentPropertyAndNotify)),
                typeof(Transform).GetProperty(nameof(Transform.parent))!.GetSetMethod());
            hooksRegistry.RegisterHook(typeof(TransformHooks).GetMethod(nameof(SetSiblingIndexAndNotify)),
                typeof(Transform).GetMethod(nameof(Transform.SetSiblingIndex), new[] {typeof(int)}));
            hooksRegistry.RegisterHook(typeof(TransformHooks).GetMethod(nameof(SetAsLastSiblingAndNotify)),
                typeof(Transform).GetMethod(nameof(Transform.SetAsLastSibling)));
            hooksRegistry.RegisterHook(typeof(TransformHooks).GetMethod(nameof(SetAsFirstSiblingAndNotify)),
                typeof(Transform).GetMethod(nameof(Transform.SetAsFirstSibling)));
        }
        
        private static void NotifyParentChanged(Transform t, Transform parent)
        {
            try
            {
                OnParentChanged(t, parent);
            }
            catch (Exception e)
            {
                HookErrors.Report($"{nameof(TransformHooks)}.{nameof(OnParentChanged)}",
                    "Failed to record a parent change. The application is not affected.", e);
            }
        }

        private static void NotifySiblingIndexChanged(Transform t, int siblingIdx)
        {
            try
            {
                OnSiblingIndexChanged(t, siblingIdx);
            }
            catch (Exception e)
            {
                HookErrors.Report($"{nameof(TransformHooks)}.{nameof(OnSiblingIndexChanged)}",
                    "Failed to record a sibling index change. The application is not affected.", e);
            }
        }

        public static void SetParentAndNotify(Transform t, Transform parent)
        {
            // A null or destroyed transform is left to the user's call, so that Unity raises its own exception (same
            // below).
            if (t == null)
            {
                t.SetParent(parent);
                return;
            }

            var previousParent = t.parent;
            t.SetParent(parent);
            if (parent != previousParent)
                NotifyParentChanged(t, parent);
        }

        public static void SetParentAndNotify(Transform t, Transform parent, bool worldPositionStays)
        {
            if (t == null)
            {
                t.SetParent(parent, worldPositionStays);
                return;
            }

            var previousParent = t.parent;
            t.SetParent(parent, worldPositionStays);
            if (parent != previousParent)
                NotifyParentChanged(t, parent);
        }

        public static void SetParentPropertyAndNotify(Transform t, Transform parent)
        {
            if (t == null)
            {
                t.parent = parent;
                return;
            }

            var previousParent = t.parent;
            t.parent = parent;
            if (parent != previousParent)
                NotifyParentChanged(t, parent);
        }

        public static void SetSiblingIndexAndNotify(Transform t, int siblingIdx)
        {
            if (t == null)
            {
                t.SetSiblingIndex(siblingIdx);
                return;
            }

            var previousSiblingIndex = t.GetSiblingIndex();
            t.SetSiblingIndex(siblingIdx);
            if (siblingIdx != previousSiblingIndex)
                NotifySiblingIndexChanged(t, siblingIdx);
        }

        public static void SetAsLastSiblingAndNotify(Transform t)
        {
            if (t == null)
            {
                t.SetAsLastSibling();
                return;
            }

            var previousSiblingIndex = t.GetSiblingIndex();
            t.SetAsLastSibling();
            var siblingIdx = t.GetSiblingIndex();
            if (siblingIdx != previousSiblingIndex)
                NotifySiblingIndexChanged(t, siblingIdx);
        }

        public static void SetAsFirstSiblingAndNotify(Transform t)
        {
            if (t == null)
            {
                t.SetAsFirstSibling();
                return;
            }

            var previousSiblingIndex = t.GetSiblingIndex();
            t.SetAsFirstSibling();
            var siblingIdx = t.GetSiblingIndex();
            if (siblingIdx != previousSiblingIndex)
                NotifySiblingIndexChanged(t, siblingIdx);
        }
    }
}