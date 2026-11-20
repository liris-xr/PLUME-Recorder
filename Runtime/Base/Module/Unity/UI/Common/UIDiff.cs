using UnityEngine;

namespace PLUME.Base.Module.Unity.UI.Common
{
    /// <summary>
    /// Change tests used by the polled UI modules. Each returns true when there is no previous value, so a first poll
    /// records everything. Floats use a small tolerance so layout round-off does not produce a sample every frame.
    /// </summary>
    public static class UIDiff
    {
        public const float Epsilon = 1e-4f;

        public static bool Changed(bool hasPrevious, bool previous, bool current) =>
            !hasPrevious || previous != current;

        public static bool Changed(bool hasPrevious, int previous, int current) =>
            !hasPrevious || previous != current;

        public static bool Changed(bool hasPrevious, float previous, float current) =>
            !hasPrevious || Mathf.Abs(previous - current) > Epsilon;

        public static bool Changed(bool hasPrevious, Vector2 previous, Vector2 current) =>
            !hasPrevious || Changed(true, previous.x, current.x) || Changed(true, previous.y, current.y);

        public static bool Changed(bool hasPrevious, Vector4 previous, Vector4 current) =>
            !hasPrevious ||
            Changed(true, previous.x, current.x) || Changed(true, previous.y, current.y) ||
            Changed(true, previous.z, current.z) || Changed(true, previous.w, current.w);

        public static bool Changed(bool hasPrevious, Color previous, Color current) =>
            !hasPrevious ||
            Changed(true, previous.r, current.r) || Changed(true, previous.g, current.g) ||
            Changed(true, previous.b, current.b) || Changed(true, previous.a, current.a);

        public static bool Changed(bool hasPrevious, Rect previous, Rect current) =>
            !hasPrevious ||
            Changed(true, previous.x, current.x) || Changed(true, previous.y, current.y) ||
            Changed(true, previous.width, current.width) || Changed(true, previous.height, current.height);

        public static bool Changed(bool hasPrevious, string previous, string current) =>
            !hasPrevious || !string.Equals(previous, current);

        /// <summary>Reference comparison: a different asset or component instance counts as a change.</summary>
        public static bool Changed(bool hasPrevious, Object previous, Object current) =>
            !hasPrevious || !ReferenceEquals(previous, current);

        public static Vector4 ToVector4(RectOffset offset) =>
            offset == null ? Vector4.zero : new Vector4(offset.left, offset.bottom, offset.right, offset.top);
    }
}
