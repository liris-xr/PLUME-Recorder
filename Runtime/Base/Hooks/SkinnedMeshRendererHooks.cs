using System;
using PLUME.Core;
using UnityEngine;

namespace PLUME.Base.Hooks
{
    public static class SkinnedMeshRendererHooks
    {
        //Create a MeshRendererHooks like the MeshFilterHooks
        public static Action<SkinnedMeshRenderer, Material> OnSetMaterial;

        public static Action<SkinnedMeshRenderer, Material> OnSetSharedMaterial;
        
        public static Action<SkinnedMeshRenderer, Material[]> OnSetMaterials;

        public static Action<SkinnedMeshRenderer, Material[]> OnSetSharedMaterials;
        
        public static Action<SkinnedMeshRenderer, Transform[]> OnSetBones;
        
        public static Action<SkinnedMeshRenderer, Bounds> OnSetLocalBounds;
        
        public static Action<SkinnedMeshRenderer, Bounds> OnSetWorldBounds;
        
        public static Action<SkinnedMeshRenderer, int> OnSetLightmapIndex;
        
        public static Action<SkinnedMeshRenderer, Vector4> OnSetLightmapScaleOffset;
        
        public static Action<SkinnedMeshRenderer, int> OnSetRealtimeLightmapIndex;
        
        public static Action<SkinnedMeshRenderer, Vector4> OnSetRealtimeLightmapScaleOffset;
        
        

        [RegisterHookAfterPropertySetter(typeof(SkinnedMeshRenderer), nameof(SkinnedMeshRenderer.sharedMaterial))]
        public static void SetSharedMaterialHook(SkinnedMeshRenderer skinnedMeshRenderer, Material sharedMaterial)
        {
            OnSetSharedMaterial?.Invoke(skinnedMeshRenderer, sharedMaterial);
        }

        [RegisterHookAfterPropertySetter(typeof(SkinnedMeshRenderer), nameof(SkinnedMeshRenderer.material))]
        public static void SetMaterialHook(SkinnedMeshRenderer skinnedMeshRenderer, Material material)
        {
            OnSetMaterial?.Invoke(skinnedMeshRenderer, material);
        }
        
        [RegisterHookAfterPropertySetter(typeof(SkinnedMeshRenderer), nameof(SkinnedMeshRenderer.sharedMaterials))]
        public static void SetSharedMaterialsHook(SkinnedMeshRenderer skinnedMeshRenderer, Material[] sharedMaterials)
        {
            OnSetSharedMaterials?.Invoke(skinnedMeshRenderer, sharedMaterials);
        }

        [RegisterHookAfterPropertySetter(typeof(SkinnedMeshRenderer), nameof(SkinnedMeshRenderer.materials))]
        public static void SetMaterialsHook(SkinnedMeshRenderer skinnedMeshRenderer, Material[] materials)
        {
            OnSetMaterials?.Invoke(skinnedMeshRenderer, materials);
        }
        
        [RegisterHookAfterPropertySetter(typeof(SkinnedMeshRenderer), nameof(SkinnedMeshRenderer.bones))]
        public static void SetBonesHook(SkinnedMeshRenderer skinnedMeshRenderer, Transform[] bones)
        {
            OnSetBones?.Invoke(skinnedMeshRenderer, bones);
        }
        
        [RegisterHookAfterPropertySetter(typeof(SkinnedMeshRenderer), nameof(SkinnedMeshRenderer.localBounds))]
        public static void SetLocalBoundsHook(SkinnedMeshRenderer skinnedMeshRenderer, Bounds localBounds)
        {
            OnSetLocalBounds?.Invoke(skinnedMeshRenderer, localBounds);
        }
        
        [RegisterHookAfterPropertySetter(typeof(SkinnedMeshRenderer), nameof(SkinnedMeshRenderer.bounds))]
        public static void SetWorldBoundsHook(SkinnedMeshRenderer skinnedMeshRenderer, Bounds worldBounds)
        {
            OnSetWorldBounds?.Invoke(skinnedMeshRenderer, worldBounds);
        }

        [RegisterHookAfterPropertySetter(typeof(SkinnedMeshRenderer), nameof(SkinnedMeshRenderer.lightmapIndex))]
        public static void SetLightmapIndexHook(SkinnedMeshRenderer skinnedMeshRenderer, int lightmapIndex)
        {
            OnSetLightmapIndex?.Invoke(skinnedMeshRenderer, lightmapIndex);
        }
        
        [RegisterHookAfterPropertySetter(typeof(SkinnedMeshRenderer), nameof(SkinnedMeshRenderer.lightmapScaleOffset))]
        public static void SetLightmapScaleOffsetHook(SkinnedMeshRenderer skinnedMeshRenderer, Vector4 lightmapScaleOffset)
        {
            OnSetLightmapScaleOffset?.Invoke(skinnedMeshRenderer, lightmapScaleOffset);
        }
        
        [RegisterHookAfterPropertySetter(typeof(SkinnedMeshRenderer), nameof(SkinnedMeshRenderer.realtimeLightmapIndex))]
        public static void SetRealtimeLightmapIndexHook(SkinnedMeshRenderer skinnedMeshRenderer, int realtimeLightmapIndex)
        {
            OnSetRealtimeLightmapIndex?.Invoke(skinnedMeshRenderer, realtimeLightmapIndex);
        }
        
        [RegisterHookAfterPropertySetter(typeof(SkinnedMeshRenderer), nameof(SkinnedMeshRenderer.realtimeLightmapScaleOffset))]
        public static void SetRealtimeLightmapScaleOffsetHook(SkinnedMeshRenderer skinnedMeshRenderer, Vector4 realtimeLightmapScaleOffset)
        {
            OnSetRealtimeLightmapScaleOffset?.Invoke(skinnedMeshRenderer, realtimeLightmapScaleOffset);
        }
        
        
    }
}