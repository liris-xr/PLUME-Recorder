using System;
using System.Collections.Generic;
using System.Linq;
using PLUME.Core;
using PLUME.Core.Hooks;
using UnityEngine;
using UnityEngine.Scripting;

namespace PLUME.Base.Hooks
{
    [Preserve]
    public class SkinnedMeshRendererHooks : IRegisterHooksCallback
    {
        public delegate void OnBonesChangedDelegate(SkinnedMeshRenderer renderer, IEnumerable<Transform> bones);

        public delegate void OnRootBoneChangedDelegate(SkinnedMeshRenderer renderer, Transform rootBone);

        public delegate void OnBlendShapeWeightChangedDelegate(SkinnedMeshRenderer renderer, int index, float value);

        public static event OnBonesChangedDelegate OnBonesChanged = delegate { };
        public static event OnRootBoneChangedDelegate OnRootBoneChanged = delegate { };
        public static event OnBlendShapeWeightChangedDelegate OnBlendShapeWeightChanged = delegate { };
        
        public void RegisterHooks(HooksRegistry hooksRegistry)
        {
            hooksRegistry.RegisterHook(typeof(SkinnedMeshRendererHooks).GetMethod(nameof(SetBonesAndNotify)),
                typeof(SkinnedMeshRenderer).GetProperty(nameof(SkinnedMeshRenderer.bones))!.GetSetMethod());
            
            hooksRegistry.RegisterHook(typeof(SkinnedMeshRendererHooks).GetMethod(nameof(SetRootBoneAndNotify)),
                typeof(SkinnedMeshRenderer).GetProperty(nameof(SkinnedMeshRenderer.rootBone))!.GetSetMethod());
            
            hooksRegistry.RegisterHook(typeof(SkinnedMeshRendererHooks).GetMethod(nameof(SetBlendShapeWeightAndNotify)),
                typeof(SkinnedMeshRenderer).GetMethod(nameof(SkinnedMeshRenderer.SetBlendShapeWeight), new[] {typeof(int), typeof(float)}));
        }
        
        private static void NotifyBonesChanged(SkinnedMeshRenderer renderer, IEnumerable<Transform> bones)
        {
            try
            {
                OnBonesChanged(renderer, bones);
            }
            catch (Exception e)
            {
                ErrorReporter.Report($"{nameof(SkinnedMeshRendererHooks)}.{nameof(OnBonesChanged)}",
                    "Failed to record a bones change. The application is not affected.", e);
            }
        }

        private static void NotifyRootBoneChanged(SkinnedMeshRenderer renderer, Transform rootBone)
        {
            try
            {
                OnRootBoneChanged(renderer, rootBone);
            }
            catch (Exception e)
            {
                ErrorReporter.Report($"{nameof(SkinnedMeshRendererHooks)}.{nameof(OnRootBoneChanged)}",
                    "Failed to record a root bone change. The application is not affected.", e);
            }
        }

        private static void NotifyBlendShapeWeightChanged(SkinnedMeshRenderer renderer, int index, float value)
        {
            try
            {
                OnBlendShapeWeightChanged(renderer, index, value);
            }
            catch (Exception e)
            {
                ErrorReporter.Report($"{nameof(SkinnedMeshRendererHooks)}.{nameof(OnBlendShapeWeightChanged)}",
                    "Failed to record a blend shape weight change. The application is not affected.", e);
            }
        }

        public static void SetBonesAndNotify(SkinnedMeshRenderer skinnedMeshRenderer, Transform[] bones)
        {
            // Invalid arguments are left to the user's call, so that Unity raises its own exception (same below).
            if (skinnedMeshRenderer == null || bones == null)
            {
                skinnedMeshRenderer.bones = bones;
                return;
            }

            var previousBones = skinnedMeshRenderer.bones;
            skinnedMeshRenderer.bones = bones;
            if (!previousBones.SequenceEqual(bones))
            {
                NotifyBonesChanged(skinnedMeshRenderer, bones);
            }
        }

        public static void SetRootBoneAndNotify(SkinnedMeshRenderer skinnedMeshRenderer, Transform rootBone)
        {
            if (skinnedMeshRenderer == null)
            {
                skinnedMeshRenderer.rootBone = rootBone;
                return;
            }

            var previousRootBone = skinnedMeshRenderer.rootBone;
            skinnedMeshRenderer.rootBone = rootBone;
            if (previousRootBone != rootBone)
            {
                NotifyRootBoneChanged(skinnedMeshRenderer, skinnedMeshRenderer.rootBone);
            }
        }

        public static void SetBlendShapeWeightAndNotify(SkinnedMeshRenderer skinnedMeshRenderer, int index,
            float weight)
        {
            if (skinnedMeshRenderer == null)
            {
                skinnedMeshRenderer.SetBlendShapeWeight(index, weight);
                return;
            }

            var previousBlenderShapeWeight = skinnedMeshRenderer.GetBlendShapeWeight(index);
            skinnedMeshRenderer.SetBlendShapeWeight(index, weight);
            if (Math.Abs(previousBlenderShapeWeight - weight) > Mathf.Epsilon)
            {
                NotifyBlendShapeWeightChanged(skinnedMeshRenderer, index, weight);
            }
        }
    }
}