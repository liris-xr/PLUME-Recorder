using System;
using PLUME.Core;
using UnityEngine;
using UnityEngine.Scripting;

namespace PLUME.Base.Hooks
{
    public static class SkinnedMeshRendererHooks
    {
        public static Action<SkinnedMeshRenderer, Transform[]> OnSetBones;

        [Preserve]
        [RegisterHookAfterPropertySetter(typeof(SkinnedMeshRenderer), nameof(SkinnedMeshRenderer.bones))]
        public static void SetBonesHook(SkinnedMeshRenderer skinnedMeshRenderer, Transform[] bones)
        {
            OnSetBones?.Invoke(skinnedMeshRenderer, bones);
        }
    }
}