using System;
using System.Collections.Generic;
using System.Linq;
using PLUME.Core;
using UnityEngine;
using UnityEngine.Scripting;

namespace PLUME.Base.Events
{
    public static class SkinnedMeshRendererEvents
    {
        public delegate void OnBonesChangedDelegate(SkinnedMeshRenderer renderer, IEnumerable<Transform> bones);

        public delegate void OnRootBoneChangedDelegate(SkinnedMeshRenderer renderer, Transform rootBone);

        public delegate void OnBlendShapeWeightChangedDelegate(SkinnedMeshRenderer renderer, int index, float value);

        public static event OnBonesChangedDelegate OnBonesChanged = delegate { };
        public static event OnRootBoneChangedDelegate OnRootBoneChanged = delegate { };
        public static event OnBlendShapeWeightChangedDelegate OnBlendShapeWeightChanged = delegate { };

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

        [Preserve]
        [RegisterMethodDetour(typeof(SkinnedMeshRenderer), nameof(SkinnedMeshRenderer.SetBlendShapeWeight), typeof(int),
            typeof(float))]
        public static void SetBlendShapeWeightAndNotify(SkinnedMeshRenderer skinnedMeshRenderer, int index,
            float weight)
        {
            var previousBlenderShapeWeight = skinnedMeshRenderer.GetBlendShapeWeight(index);
            skinnedMeshRenderer.SetBlendShapeWeight(index, weight);
            if (Math.Abs(previousBlenderShapeWeight - weight) > Mathf.Epsilon)
            {
                OnBlendShapeWeightChanged(skinnedMeshRenderer, index, weight);
            }
        }
    }
}