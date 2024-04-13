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
        
        public static void SetParentAndNotify(Transform t, Transform parent)
        {
            var previousParent = t.parent;
            t.SetParent(parent);
            if (parent != previousParent)
                OnParentChanged(t, parent);
        }

        public static void SetParentAndNotify(Transform t, Transform parent, bool worldPositionStays)
        {
            var previousParent = t.parent;
            t.SetParent(parent, worldPositionStays);
            if (parent != previousParent)
                OnParentChanged(t, parent);
        }

        public static void SetParentPropertyAndNotify(Transform t, Transform parent)
        {
            var previousParent = t.parent;
            t.parent = parent;
            if (parent != previousParent)
                OnParentChanged(t, parent);
        }

        public static void SetSiblingIndexAndNotify(Transform t, int siblingIdx)
        {
            var previousSiblingIndex = t.GetSiblingIndex();
            t.SetSiblingIndex(siblingIdx);
            if (siblingIdx != previousSiblingIndex)
                OnSiblingIndexChanged(t, siblingIdx);
        }

        public static void SetAsLastSiblingAndNotify(Transform t)
        {
            var previousSiblingIndex = t.GetSiblingIndex();
            t.SetAsLastSibling();
            var siblingIdx = t.GetSiblingIndex();
            if (siblingIdx != previousSiblingIndex)
                OnSiblingIndexChanged(t, siblingIdx);
        }

        public static void SetAsFirstSiblingAndNotify(Transform t)
        {
            var previousSiblingIndex = t.GetSiblingIndex();
            t.SetAsFirstSibling();
            var siblingIdx = t.GetSiblingIndex();
            if (siblingIdx != previousSiblingIndex)
                OnSiblingIndexChanged(t, siblingIdx);
        }
    }
}