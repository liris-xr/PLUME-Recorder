using System;
using UnityEngine;

namespace PLUME.Base.Events
{
    public static class SkinnedMeshRendererEvents
    {
        public static Action<SkinnedMeshRenderer, Transform[]> OnSetBones;

        // [Preserve]
        // [RegisterMethodDetourAfterPropertySetter(typeof(SkinnedMeshRenderer), nameof(SkinnedMeshRenderer.bones))]
        // public static void SetBonesHook(SkinnedMeshRenderer skinnedMeshRenderer, Transform[] bones)
        // {
        //     OnSetBones?.Invoke(skinnedMeshRenderer, bones);
        // }
    }
}