using System.Collections.Generic;
using System.Linq;
using PLUME.Core;
using UnityEngine;
using UnityEngine.Scripting;

namespace PLUME.Base.Events
{
    public static class SkinnedMeshRendererEvents
    {
        public delegate void OnBonesChangedDelegate(Renderer renderer, IEnumerable<Transform> bones);

        public delegate void OnRootBoneChangedDelegate(Renderer renderer, Transform rootBone);

        public static event OnBonesChangedDelegate OnBonesChanged = delegate { };
        public static event OnRootBoneChangedDelegate OnRootBoneChanged = delegate { };

        [Preserve]
        [RegisterPropertySetterDetour(typeof(SkinnedMeshRenderer), nameof(SkinnedMeshRenderer.bones))]
        public static void SetBonesAndNotify(SkinnedMeshRenderer skinnedMeshRenderer, Transform[] bones)
        {
            var previousBones = skinnedMeshRenderer.bones;
            skinnedMeshRenderer.bones = bones;
            if (!previousBones.SequenceEqual(bones))
            {
                OnBonesChanged(skinnedMeshRenderer, bones);
            }
        }

        [Preserve]
        [RegisterPropertySetterDetour(typeof(SkinnedMeshRenderer), nameof(SkinnedMeshRenderer.rootBone))]
        public static void SetRootBoneAndNotify(SkinnedMeshRenderer skinnedMeshRenderer, Transform rootBone)
        {
            var previousRootBone = skinnedMeshRenderer.rootBone;
            skinnedMeshRenderer.rootBone = rootBone;
            if (previousRootBone != rootBone)
            {
                OnRootBoneChanged(skinnedMeshRenderer, skinnedMeshRenderer.rootBone);
            }
        }
    }
}