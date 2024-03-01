using System;
using System.Collections.Generic;
using PLUME.Core;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Scripting;

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

        public static Action<Renderer, Bounds> OnSetLocalBounds;

        public static Action<Renderer> OnResetLocalBounds;

        public static Action<Renderer, MaterialPropertyBlock, int> OnSetPropertyBlockMaterialIndex;
        
        public static Action<Renderer, MaterialPropertyBlock> OnSetPropertyBlock;
        
        [Preserve]
        [RegisterHookAfterPropertySetter(typeof(Renderer), nameof(Renderer.material))]
        public static void SetMaterialHook(Renderer renderer, Material material)
        {
            OnSetMaterial?.Invoke(renderer, material);
        }
        
        [Preserve]
        [RegisterHookAfterPropertySetter(typeof(Renderer), nameof(Renderer.materials))]
        public static void SetMaterialsHook(Renderer renderer, IEnumerable<Material> materials)
        {
            OnSetMaterials?.Invoke(renderer, materials);
        }
        
        [Preserve]
        [RegisterHookAfterPropertySetter(typeof(Renderer), nameof(Renderer.sharedMaterial))]
        public static void SetSharedMaterialHook(Renderer renderer, Material sharedMaterial)
        {
            OnSetSharedMaterial?.Invoke(renderer, sharedMaterial);
        }
        
        [Preserve]
        [RegisterHookAfterPropertySetter(typeof(Renderer), nameof(Renderer.sharedMaterials))]
        public static void SetSharedMaterialsHook(Renderer renderer, IEnumerable<Material> sharedMaterials)
        {
            OnSetSharedMaterials?.Invoke(renderer, sharedMaterials);
        }
        
        [Preserve]
        [RegisterHookAfterPropertySetter(typeof(Renderer), nameof(Renderer.enabled))]
        public static void SetEnabledHook(Renderer renderer, bool enabled)
        {
            OnSetEnabled?.Invoke(renderer, enabled);
        }
        
        [Preserve]
        [RegisterHookAfterPropertySetter(typeof(Renderer), nameof(Renderer.localBounds))]
        public static void SetLocalBoundsHook(Renderer renderer, Bounds localBounds)
        {
            OnSetLocalBounds?.Invoke(renderer, localBounds);
        }
        
        [Preserve]
        [RegisterHookAfterMethod(typeof(Renderer), nameof(Renderer.ResetLocalBounds))]
        public static void ResetLocalBoundsHook(Renderer renderer)
        {
            OnResetLocalBounds?.Invoke(renderer);
        }
        
        [Preserve]
        [RegisterHookAfterPropertySetter(typeof(Renderer), nameof(Renderer.lightmapIndex))]
        public static void SetLightmapIndexHook(Renderer renderer, int lightmapIndex)
        {
            OnSetLightmapIndex?.Invoke(renderer, lightmapIndex);
        }
        
        [Preserve]
        [RegisterHookAfterPropertySetter(typeof(Renderer), nameof(Renderer.realtimeLightmapIndex))]
        public static void SetRealtimeLightmapIndexHook(Renderer renderer, int realtimeLightmapIndex)
        {
            OnSetRealtimeLightmapIndex?.Invoke(renderer, realtimeLightmapIndex);
        }
        
        [Preserve]
        [RegisterHookAfterPropertySetter(typeof(Renderer), nameof(Renderer.lightmapScaleOffset))]
        public static void SetLightmapScaleOffsetHook(Renderer renderer, Vector4 lightmapScaleOffset)
        {
            OnSetLightmapScaleOffset?.Invoke(renderer, lightmapScaleOffset);
        }
        
        [Preserve]
        [RegisterHookAfterPropertySetter(typeof(Renderer), nameof(Renderer.realtimeLightmapScaleOffset))]
        public static void SetRealtimeLightmapScaleOffsetHook(Renderer renderer, Vector4 realtimeLightmapScaleOffset)
        {
            OnSetRealtimeLightmapScaleOffset?.Invoke(renderer, realtimeLightmapScaleOffset);
        }
        
        [Preserve]
        [RegisterHookAfterPropertySetter(typeof(Renderer), nameof(Renderer.receiveShadows))]
        public static void SetReceiveShadowsHook(Renderer renderer, bool receiveShadows)
        {
            OnSetReceiveShadows?.Invoke(renderer, receiveShadows);
        }
        
        [Preserve]
        [RegisterHookAfterPropertySetter(typeof(Renderer), nameof(Renderer.forceRenderingOff))]
        public static void SetForceRenderingOffHook(Renderer renderer, bool forceRenderingOff)
        {
            OnSetForceRenderingOff?.Invoke(renderer, forceRenderingOff);
        }
        
        [Preserve]
        [RegisterHookAfterPropertySetter(typeof(Renderer), nameof(Renderer.staticShadowCaster))]
        public static void SetStaticShadowCasterHook(Renderer renderer, bool staticShadowCaster)
        {
            OnSetStaticShadowCaster?.Invoke(renderer, staticShadowCaster);
        }
        
        [Preserve]
        [RegisterHookAfterPropertySetter(typeof(Renderer), nameof(Renderer.motionVectorGenerationMode))]
        public static void SetMotionVectorGenerationModeHook(Renderer renderer, MotionVectorGenerationMode motionVectorGenerationMode)
        {
            OnSetMotionVectorGenerationMode?.Invoke(renderer, motionVectorGenerationMode);
        }
        
        [Preserve]
        [RegisterHookAfterPropertySetter(typeof(Renderer), nameof(Renderer.lightProbeUsage))]
        public static void SetLightProbeUsageHook(Renderer renderer, LightProbeUsage lightProbeUsage)
        {
            OnSetLightProbeUsage?.Invoke(renderer, lightProbeUsage);
        }
        
        [Preserve]
        [RegisterHookAfterPropertySetter(typeof(Renderer), nameof(Renderer.reflectionProbeUsage))]
        public static void SetReflectionProbeUsageHook(Renderer renderer, ReflectionProbeUsage reflectionProbeUsage)
        {
            OnSetReflectionProbeUsage?.Invoke(renderer, reflectionProbeUsage);
        }
        
        [Preserve]
        [RegisterHookAfterPropertySetter(typeof(Renderer), nameof(Renderer.renderingLayerMask))]
        public static void SetRenderingLayerMaskHook(Renderer renderer, uint renderingLayerMask)
        {
            OnSetRenderingLayerMask?.Invoke(renderer, renderingLayerMask);
        }
        
        [Preserve]
        [RegisterHookAfterPropertySetter(typeof(Renderer), nameof(Renderer.rayTracingMode))]
        public static void SetRayTracingModeHook(Renderer renderer, RayTracingMode rayTracingMode)
        {
            OnSetRayTracingMode?.Invoke(renderer, rayTracingMode);
        }
        
        [Preserve]
        [RegisterHookAfterPropertySetter(typeof(Renderer), nameof(Renderer.sortingLayerName))]
        public static void SetSortingLayerNameHook(Renderer renderer, string sortingLayerName)
        {
            OnSetSortingLayerName?.Invoke(renderer, sortingLayerName);
        }
        
        [Preserve]
        [RegisterHookAfterPropertySetter(typeof(Renderer), nameof(Renderer.sortingLayerID))]
        public static void SetSortingLayerIDHook(Renderer renderer, int sortingLayerID)
        {
            OnSetSortingLayerID?.Invoke(renderer, sortingLayerID);
        }
        
        [Preserve]
        [RegisterHookAfterPropertySetter(typeof(Renderer), nameof(Renderer.sortingOrder))]
        public static void SetSortingOrderHook(Renderer renderer, int sortingOrder)
        {
            OnSetSortingOrder?.Invoke(renderer, sortingOrder);
        }
        
        [Preserve]
        [RegisterHookAfterPropertySetter(typeof(Renderer), nameof(Renderer.lightProbeProxyVolumeOverride))]
        public static void SetLightProbeProxyVolumeOverrideHook(Renderer renderer, GameObject lightProbeProxyVolumeOverride)
        {
            OnSetLightProbeProxyVolumeOverride?.Invoke(renderer, lightProbeProxyVolumeOverride);
        }
        
        [Preserve]
        [RegisterHookAfterPropertySetter(typeof(Renderer), nameof(Renderer.probeAnchor))]
        public static void SetProbeAnchorHook(Renderer renderer, Transform probeAnchor)
        {
            OnSetProbeAnchor?.Invoke(renderer, probeAnchor);
        }
        
        [Preserve]
        [RegisterHookAfterMethod(typeof(Renderer), nameof(Renderer.SetPropertyBlock), typeof(MaterialPropertyBlock))]
        public static void SetPropertyBlockHook(Renderer renderer, MaterialPropertyBlock properties)
        {
            OnSetPropertyBlock?.Invoke(renderer, properties);
        }
        
        [Preserve]
        [RegisterHookAfterMethod(typeof(Renderer), nameof(Renderer.SetPropertyBlock), typeof(MaterialPropertyBlock), typeof(int))]
        public static void SetPropertyBlockHook(Renderer renderer, MaterialPropertyBlock properties, int materialIndex)
        {
            OnSetPropertyBlockMaterialIndex?.Invoke(renderer, properties, materialIndex);
        }
    }
}