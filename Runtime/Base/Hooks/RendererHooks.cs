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
    public class RendererHooks : IRegisterHooksCallback
    {
        public delegate void OnEnabledChangedDelegate(Renderer renderer, bool enabled);

        public delegate void OnMaterialChangedDelegate(Renderer renderer, Material material);

        public delegate void OnMaterialsChangedDelegate(Renderer renderer, IEnumerable<Material> materials);

        public delegate void OnPropertyBlockChangedDelegate(Renderer renderer, MaterialPropertyBlock properties);

        public delegate void OnPropertyBlockMaterialIndexChangedDelegate(Renderer renderer,
            MaterialPropertyBlock properties, int materialIndex);

        public static event OnEnabledChangedDelegate OnEnabledChanged = delegate { };
        public static event OnMaterialChangedDelegate OnMaterialChanged = delegate { };
        public static event OnMaterialChangedDelegate OnSharedMaterialChanged = delegate { };
        public static event OnMaterialsChangedDelegate OnMaterialsChanged = delegate { };
        public static event OnMaterialsChangedDelegate OnSharedMaterialsChanged = delegate { };
        public static event OnPropertyBlockChangedDelegate OnPropertyBlockChanged = delegate { };
        public static event OnPropertyBlockMaterialIndexChangedDelegate OnPropertyBlockMaterialIndexChanged =
            delegate { };
        
        public void RegisterHooks(HooksRegistry hooksRegistry)
        {
            hooksRegistry.RegisterHook(typeof(RendererHooks).GetMethod(nameof(SetEnabledAndNotify)),
                typeof(Renderer).GetProperty(nameof(Renderer.enabled))!.GetSetMethod());

            hooksRegistry.RegisterHook(typeof(RendererHooks).GetMethod(nameof(SetMaterialPropertyAndNotify)),
                typeof(Renderer).GetProperty(nameof(Renderer.material))!.GetSetMethod());

            hooksRegistry.RegisterHook(typeof(RendererHooks).GetMethod(nameof(SetMaterialsPropertyAndNotify)),
                typeof(Renderer).GetProperty(nameof(Renderer.materials))!.GetSetMethod());

            hooksRegistry.RegisterHook(typeof(RendererHooks).GetMethod(nameof(SetSharedMaterialAndNotify)),
                typeof(Renderer).GetProperty(nameof(Renderer.sharedMaterial))!.GetSetMethod());

            hooksRegistry.RegisterHook(typeof(RendererHooks).GetMethod(nameof(SetSharedMaterialsPropertyAndNotify)),
                typeof(Renderer).GetProperty(nameof(Renderer.sharedMaterials))!.GetSetMethod());

            hooksRegistry.RegisterHook(typeof(RendererHooks).GetMethod(nameof(SetPropertyBlockAndNotify),
                new[] {typeof(Renderer), typeof(MaterialPropertyBlock)}),
                typeof(Renderer).GetMethod(nameof(Renderer.SetPropertyBlock), new[] {typeof(MaterialPropertyBlock)}));

            hooksRegistry.RegisterHook(typeof(RendererHooks).GetMethod(nameof(SetPropertyBlockAndNotify),
                new[] {typeof(Renderer), typeof(MaterialPropertyBlock), typeof(int)}),
                typeof(Renderer).GetMethod(nameof(Renderer.SetPropertyBlock), new[] {typeof(MaterialPropertyBlock), typeof(int)}));

            hooksRegistry.RegisterHook(typeof(RendererHooks).GetMethod(nameof(SetMaterialsAndNotify)),
                typeof(Renderer).GetMethod(nameof(Renderer.SetMaterials), new[] {typeof(List<Material>)}));

            hooksRegistry.RegisterHook(typeof(RendererHooks).GetMethod(nameof(SetSharedMaterialsAndNotify)),
                typeof(Renderer).GetMethod(nameof(Renderer.SetSharedMaterials), new[] {typeof(List<Material>)}));
        }
        
        private static void NotifyEnabledChanged(Renderer renderer, bool enabled)
        {
            try
            {
                OnEnabledChanged(renderer, enabled);
            }
            catch (Exception e)
            {
                ErrorReporter.Report($"{nameof(RendererHooks)}.{nameof(OnEnabledChanged)}",
                    "Failed to record a renderer enabled state change. The application is not affected.", e);
            }
        }

        private static void NotifySharedMaterialChanged(Renderer renderer, Material material)
        {
            try
            {
                OnSharedMaterialChanged(renderer, material);
            }
            catch (Exception e)
            {
                ErrorReporter.Report($"{nameof(RendererHooks)}.{nameof(OnSharedMaterialChanged)}",
                    "Failed to record a shared material change. The application is not affected.", e);
            }
        }

        private static void NotifySharedMaterialsChanged(Renderer renderer, IEnumerable<Material> materials)
        {
            try
            {
                OnSharedMaterialsChanged(renderer, materials);
            }
            catch (Exception e)
            {
                ErrorReporter.Report($"{nameof(RendererHooks)}.{nameof(OnSharedMaterialsChanged)}",
                    "Failed to record a shared materials change. The application is not affected.", e);
            }
        }

        private static void NotifyMaterialChanged(Renderer renderer, Material material)
        {
            try
            {
                OnMaterialChanged(renderer, material);
            }
            catch (Exception e)
            {
                ErrorReporter.Report($"{nameof(RendererHooks)}.{nameof(OnMaterialChanged)}",
                    "Failed to record a material change. The application is not affected.", e);
            }
        }

        private static void NotifyMaterialsChanged(Renderer renderer, IEnumerable<Material> materials)
        {
            try
            {
                OnMaterialsChanged(renderer, materials);
            }
            catch (Exception e)
            {
                ErrorReporter.Report($"{nameof(RendererHooks)}.{nameof(OnMaterialsChanged)}",
                    "Failed to record a materials change. The application is not affected.", e);
            }
        }

        private static void NotifyPropertyBlockMaterialIndexChanged(Renderer renderer,
            MaterialPropertyBlock properties, int materialIndex)
        {
            try
            {
                OnPropertyBlockMaterialIndexChanged(renderer, properties, materialIndex);
            }
            catch (Exception e)
            {
                ErrorReporter.Report($"{nameof(RendererHooks)}.{nameof(OnPropertyBlockMaterialIndexChanged)}",
                    "Failed to record a per-material property block change. The application is not affected.", e);
            }
        }

        private static void NotifyPropertyBlockChanged(Renderer renderer, MaterialPropertyBlock properties)
        {
            try
            {
                OnPropertyBlockChanged(renderer, properties);
            }
            catch (Exception e)
            {
                ErrorReporter.Report($"{nameof(RendererHooks)}.{nameof(OnPropertyBlockChanged)}",
                    "Failed to record a renderer property block change. The application is not affected.", e);
            }
        }

        public static void SetEnabledAndNotify(Renderer renderer, bool enabled)
        {
            // Invalid arguments are left to the user's call, so that Unity raises its own exception (same below).
            if (renderer == null)
            {
                renderer.enabled = enabled;
                return;
            }

            var previousEnabled = renderer.enabled;
            renderer.enabled = enabled;
            if (previousEnabled != enabled)
            {
                NotifyEnabledChanged(renderer, enabled);
            }
        }

        public static void SetMaterialPropertyAndNotify(Renderer renderer, Material material)
        {
            // sharedMaterial points to material if instantiated. As such, using sharedMaterial allows for detecting
            // if the material goes from shared to instantiated or from one instance to another.
            if (renderer == null)
            {
                renderer.material = material;
                return;
            }

            var previousMaterial = renderer.sharedMaterial;
            renderer.material = material;
            if (previousMaterial != material)
            {
                NotifyMaterialChanged(renderer, material);
                NotifyMaterialsChanged(renderer, renderer.sharedMaterials);
            }
        }

        public static void SetMaterialsPropertyAndNotify(Renderer renderer, Material[] materials)
        {
            if (renderer == null || materials == null)
            {
                renderer.materials = materials;
                return;
            }

            var previousMaterials = renderer.sharedMaterials;
            renderer.materials = materials;
            if (!previousMaterials.SequenceEqual(materials))
            {
                if(materials.Length > 0)
                    NotifyMaterialChanged(renderer, materials[0]);
                NotifyMaterialsChanged(renderer, materials);
            }
        }

        public static void SetMaterialsAndNotify(Renderer renderer, List<Material> materials)
        {
            if (renderer == null || materials == null)
            {
                renderer.SetMaterials(materials);
                return;
            }

            var previousMaterials = renderer.sharedMaterials;
            renderer.SetMaterials(materials);
            if (!previousMaterials.SequenceEqual(materials))
            {
                if(materials.Count > 0)
                    NotifyMaterialChanged(renderer, materials[0]);
                NotifyMaterialsChanged(renderer, materials);
            }
        }

        public static void SetSharedMaterialAndNotify(Renderer renderer, Material sharedMaterial)
        {
            if (renderer == null)
            {
                renderer.sharedMaterial = sharedMaterial;
                return;
            }

            var previousMaterial = renderer.sharedMaterial;
            renderer.sharedMaterial = sharedMaterial;
            if (previousMaterial != sharedMaterial)
            {
                NotifySharedMaterialChanged(renderer, sharedMaterial);
                NotifySharedMaterialsChanged(renderer, renderer.sharedMaterials);
            }
        }

        public static void SetSharedMaterialsPropertyAndNotify(Renderer renderer, Material[] sharedMaterials)
        {
            if (renderer == null || sharedMaterials == null)
            {
                renderer.sharedMaterials = sharedMaterials;
                return;
            }

            var previousMaterials = renderer.sharedMaterials;
            renderer.sharedMaterials = sharedMaterials;
            if (!previousMaterials.SequenceEqual(sharedMaterials))
            {
                if(sharedMaterials.Length > 0)
                    NotifySharedMaterialChanged(renderer, sharedMaterials[0]);
                NotifySharedMaterialsChanged(renderer, sharedMaterials);
            }
        }

        public static void SetSharedMaterialsAndNotify(Renderer renderer, List<Material> sharedMaterials)
        {
            if (renderer == null || sharedMaterials == null)
            {
                renderer.SetSharedMaterials(sharedMaterials);
                return;
            }

            var previousMaterials = renderer.sharedMaterials;
            renderer.SetSharedMaterials(sharedMaterials);
            if (!previousMaterials.SequenceEqual(sharedMaterials))
            {
                if(sharedMaterials.Count > 0)
                    NotifySharedMaterialChanged(renderer, sharedMaterials[0]);
                NotifySharedMaterialsChanged(renderer, sharedMaterials);
            }
        }

        public static void SetPropertyBlockAndNotify(Renderer renderer, MaterialPropertyBlock properties)
        {
            renderer.SetPropertyBlock(properties);
            NotifyPropertyBlockChanged(renderer, properties);
        }
        
        public static void SetPropertyBlockAndNotify(Renderer renderer, MaterialPropertyBlock properties, int materialIndex)
        {
            renderer.SetPropertyBlock(properties, materialIndex);
            NotifyPropertyBlockMaterialIndexChanged(renderer, properties, materialIndex);
        }
    }
}