using System;
using System.Collections.Generic;
using PLUME.Core;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace PLUME.Base.Hooks
{
    public class RendererHooks
    {
        public static Action<Renderer, Material> OnSetMaterial;

        public static Action<Renderer, IEnumerable<Material>> OnSetMaterials;

        public static Action<Renderer, Material> OnSetSharedMaterial;

        public static Action<Renderer, IEnumerable<Material>> OnSetSharedMaterials;

        public static Action<Renderer, bool> OnSetEnabled;
        
        public static Action<Renderer, bool> OnSetReceiveShadows;
        
        public static Action<Renderer, bool> OnSetForceRenderingOff;
        
        public static Action<Renderer, bool> OnSetStaticShadowCaster;
        
        public static Action<Renderer, MotionVectorGenerationMode> OnSetMotionVectorGenerationMode;
        
        public static Action<Renderer, LightProbeUsage> OnSetLightProbeUsage;
        
        public static Action<Renderer, ReflectionProbeUsage> OnSetReflectionProbeUsage;
        
        public static Action<Renderer, uint> OnSetRenderingLayerMask;
        
        public static Action<Renderer, int> OnSetRenderingPriority;
        
        public static Action<Renderer, RayTracingMode> OnSetRayTracingMode;
        
        public static Action<Renderer, string> OnSetSortingLayerName;
        
        public static Action<Renderer, int> OnSetSortingLayerID;
        
        public static Action<Renderer, int> OnSetSortingOrder;
        
        public static Action<Renderer, GameObject> OnSetLightProbeProxyVolumeOverride;
        
        public static Action<Renderer, Transform> OnSetProbeAnchor;
        
        public static Action<Renderer, int> OnSetLightmapIndex;
        
        public static Action<Renderer, int> OnSetRealtimeLightmapIndex;
        
        public static Action<Renderer, Vector4> OnSetLightmapScaleOffset;
        
        public static Action<Renderer, Vector4> OnSetRealtimeLightmapScaleOffset;

        public static Action<Renderer, Bounds> OnSetBounds;

        public static Action<Renderer, Bounds> OnSetLocalBounds;

        public static Action<Renderer> OnResetBounds;

        public static Action<Renderer> OnResetLocalBounds;

        public static Action<Renderer, MaterialPropertyBlock, int?> OnSetPropertyBlock;
        
        [RegisterHookAfterPropertySetter(typeof(Renderer), nameof(Renderer.material))]
        public static void SetMaterialHook(Renderer renderer, Material material)
        {
            OnSetMaterial?.Invoke(renderer, material);
        }
        
        [RegisterHookAfterPropertySetter(typeof(Renderer), nameof(Renderer.materials))]
        public static void SetMaterialsHook(Renderer renderer, IEnumerable<Material> materials)
        {
            OnSetMaterials?.Invoke(renderer, materials);
        }
        
        [RegisterHookAfterPropertySetter(typeof(Renderer), nameof(Renderer.sharedMaterial))]
        public static void SetSharedMaterialHook(Renderer renderer, Material sharedMaterial)
        {
            OnSetSharedMaterial?.Invoke(renderer, sharedMaterial);
        }
        
        [RegisterHookAfterPropertySetter(typeof(Renderer), nameof(Renderer.sharedMaterials))]
        public static void SetSharedMaterialsHook(Renderer renderer, IEnumerable<Material> sharedMaterials)
        {
            OnSetSharedMaterials?.Invoke(renderer, sharedMaterials);
        }
        
        [RegisterHookAfterPropertySetter(typeof(Renderer), nameof(Renderer.enabled))]
        public static void SetEnabledHook(Renderer renderer, bool enabled)
        {
            OnSetEnabled?.Invoke(renderer, enabled);
        }
    }
}