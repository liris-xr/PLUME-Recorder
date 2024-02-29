using System;
using PLUME.Core;
using UnityEngine;

namespace PLUME.Base.Hooks
{
    public static class MeshRendererHooks
    {
        public static Action<MeshRenderer, Material> OnSetMaterial;

        public static Action<MeshRenderer, Material> OnSetSharedMaterial;
        
        public static Action<MeshRenderer, Material[]> OnSetMaterials;

        public static Action<MeshRenderer, Material[]> OnSetSharedMaterials;
        
        public static Action<MeshRenderer, int> OnSetLightmapIndex;
        
        public static Action<MeshRenderer, Vector4> OnSetLightmapScaleOffset;
        
        public static Action<MeshRenderer, int> OnSetRealtimeLightmapIndex;
        
        public static Action<MeshRenderer, Vector4> OnSetRealtimeLightmapScaleOffset;
        
        

        [RegisterHookAfterPropertySetter(typeof(MeshRenderer), nameof(MeshRenderer.sharedMaterial))]
        public static void SetSharedMaterialHook(MeshRenderer meshRenderer, Material sharedMaterial)
        {
            OnSetSharedMaterial?.Invoke(meshRenderer, sharedMaterial);
        }

        [RegisterHookAfterPropertySetter(typeof(MeshRenderer), nameof(MeshRenderer.material))]
        public static void SetMaterialHook(MeshRenderer meshRenderer, Material material)
        {
            OnSetMaterial?.Invoke(meshRenderer, material);
        }
        
        [RegisterHookAfterPropertySetter(typeof(MeshRenderer), nameof(MeshRenderer.sharedMaterials))]
        public static void SetSharedMaterialsHook(MeshRenderer meshRenderer, Material[] sharedMaterials)
        {
            OnSetSharedMaterials?.Invoke(meshRenderer, sharedMaterials);
        }

        [RegisterHookAfterPropertySetter(typeof(MeshRenderer), nameof(MeshRenderer.materials))]
        public static void SetMaterialsHook(MeshRenderer meshRenderer, Material[] materials)
        {
            OnSetMaterials?.Invoke(meshRenderer, materials);
        }
        
        [RegisterHookAfterPropertySetter(typeof(MeshRenderer), nameof(MeshRenderer.lightmapIndex))]
        public static void SetLightmapIndexHook(MeshRenderer meshRenderer, int lightmapIndex)
        {
            OnSetLightmapIndex?.Invoke(meshRenderer, lightmapIndex);
        }
        
        [RegisterHookAfterPropertySetter(typeof(MeshRenderer), nameof(MeshRenderer.lightmapScaleOffset))]
        public static void SetLightmapScaleOffsetHook(MeshRenderer meshRenderer, Vector4 lightmapScaleOffset)
        {
            OnSetLightmapScaleOffset?.Invoke(meshRenderer, lightmapScaleOffset);
        }
        
        [RegisterHookAfterPropertySetter(typeof(MeshRenderer), nameof(MeshRenderer.realtimeLightmapIndex))]
        public static void SetRealtimeLightmapIndexHook(MeshRenderer meshRenderer, int realtimeLightmapIndex)
        {
            OnSetRealtimeLightmapIndex?.Invoke(meshRenderer, realtimeLightmapIndex);
        }
        
        [RegisterHookAfterPropertySetter(typeof(MeshRenderer), nameof(MeshRenderer.realtimeLightmapScaleOffset))]
        public static void SetRealtimeLightmapScaleOffsetHook(MeshRenderer meshRenderer, Vector4 realtimeLightmapScaleOffset)
        {
            OnSetRealtimeLightmapScaleOffset?.Invoke(meshRenderer, realtimeLightmapScaleOffset);
        }
        
        
    }
}