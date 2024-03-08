using PLUME.Core;
using UnityEngine;
using UnityEngine.Scripting;

namespace PLUME.Base.Events
{
    public static class TransformEvents
    {
        public delegate void OnParentChangedDelegate(Transform t, Transform parent);

        public delegate void OnSiblingIndexChangedDelegate(Transform t, int siblingIdx);

        public static event OnParentChangedDelegate OnParentChanged = delegate { };

        public static event OnSiblingIndexChangedDelegate OnSiblingIndexChanged = delegate { };

        [Preserve]
        [RegisterMethodDetour(typeof(Transform), nameof(Transform.SetParent), typeof(Transform))]
        public static void SetParentAndNotify(Transform t, Transform parent)
        {
            var previousParent = t.parent;
            t.SetParent(parent);
            if (parent != previousParent)
                OnParentChanged(t, parent);
        }

        [Preserve]
        [RegisterMethodDetour(typeof(Transform), nameof(Transform.SetParent), typeof(Transform), typeof(bool))]
        public static void SetParentAndNotify(Transform t, Transform parent, bool worldPositionStays)
        {
            var previousParent = t.parent;
            t.SetParent(parent, worldPositionStays);
            if (parent != previousParent)
                OnParentChanged(t, parent);
        }

        [Preserve]
        [RegisterPropertySetterDetour(typeof(Transform), nameof(Transform.parent))]
        public static void SetParentPropertyAndNotify(Transform t, Transform parent)
        {
            var previousParent = t.parent;
            t.parent = parent;
            if (parent != previousParent)
                OnParentChanged(t, parent);
        }

        [Preserve]
        [RegisterMethodDetour(typeof(Transform), nameof(Transform.SetSiblingIndex), typeof(int))]
        public static void SetSiblingIndexAndNotify(Transform t, int siblingIdx)
        {
            var previousSiblingIndex = t.GetSiblingIndex();
            t.SetSiblingIndex(siblingIdx);
            if (siblingIdx != previousSiblingIndex)
                OnSiblingIndexChanged(t, siblingIdx);
        }

        [Preserve]
        [RegisterMethodDetour(typeof(Transform), nameof(Transform.SetAsLastSibling))]
        public static void SetAsLastSiblingAndNotify(Transform t)
        {
            var previousSiblingIndex = t.GetSiblingIndex();
            t.SetAsLastSibling();
            var siblingIdx = t.GetSiblingIndex();
            if (siblingIdx != previousSiblingIndex)
                OnSiblingIndexChanged(t, siblingIdx);
        }

        [Preserve]
        [RegisterMethodDetour(typeof(Transform), nameof(Transform.SetAsFirstSibling))]
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