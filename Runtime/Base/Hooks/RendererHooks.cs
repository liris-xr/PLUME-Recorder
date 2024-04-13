using System.Collections.Generic;
using System.Linq;
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
        
        public static void SetEnabledAndNotify(Renderer renderer, bool enabled)
        {
            var previousEnabled = renderer.enabled;
            renderer.enabled = enabled;
            if (previousEnabled != enabled)
            {
                OnEnabledChanged(renderer, enabled);
            }
        }

        public static void SetMaterialPropertyAndNotify(Renderer renderer, Material material)
        {
            // sharedMaterial points to material if instantiated. As such, using sharedMaterial allows for detecting
            // if the material goes from shared to instantiated or from one instance to another.
            var previousMaterial = renderer.sharedMaterial;
            renderer.material = material;
            if (previousMaterial != material)
            {
                OnMaterialChanged(renderer, material);
                OnMaterialsChanged(renderer, renderer.sharedMaterials);
            }
        }

        public static void SetMaterialsPropertyAndNotify(Renderer renderer, Material[] materials)
        {
            var previousMaterials = renderer.sharedMaterials;
            renderer.materials = materials;
            if (!previousMaterials.SequenceEqual(materials))
            {
                if(materials.Length > 0)
                    OnMaterialChanged(renderer, materials[0]);
                OnMaterialsChanged(renderer, materials);
            }
        }

        public static void SetMaterialsAndNotify(Renderer renderer, List<Material> materials)
        {
            var previousMaterials = renderer.sharedMaterials;
            renderer.SetMaterials(materials);
            if (!previousMaterials.SequenceEqual(materials))
            {
                if(materials.Count > 0)
                    OnMaterialChanged(renderer, materials[0]);
                OnMaterialsChanged(renderer, materials);
            }
        }

        public static void SetSharedMaterialAndNotify(Renderer renderer, Material sharedMaterial)
        {
            var previousMaterial = renderer.sharedMaterial;
            renderer.sharedMaterial = sharedMaterial;
            if (previousMaterial != sharedMaterial)
            {
                OnSharedMaterialChanged(renderer, sharedMaterial);
                OnSharedMaterialsChanged(renderer, renderer.sharedMaterials);
            }
        }

        public static void SetSharedMaterialsPropertyAndNotify(Renderer renderer, Material[] sharedMaterials)
        {
            var previousMaterials = renderer.sharedMaterials;
            renderer.sharedMaterials = sharedMaterials;
            if (!previousMaterials.SequenceEqual(sharedMaterials))
            {
                if(sharedMaterials.Length > 0)
                    OnSharedMaterialChanged(renderer, sharedMaterials[0]);
                OnSharedMaterialsChanged(renderer, sharedMaterials);
            }
        }

        public static void SetSharedMaterialsAndNotify(Renderer renderer, List<Material> sharedMaterials)
        {
            var previousMaterials = renderer.sharedMaterials;
            renderer.SetSharedMaterials(sharedMaterials);
            if (!previousMaterials.SequenceEqual(sharedMaterials))
            {
                if(sharedMaterials.Count > 0)
                    OnSharedMaterialChanged(renderer, sharedMaterials[0]);
                OnSharedMaterialsChanged(renderer, sharedMaterials);
            }
        }

        public static void SetPropertyBlockAndNotify(Renderer renderer, MaterialPropertyBlock properties)
        {
            renderer.SetPropertyBlock(properties);
            OnPropertyBlockChanged(renderer, properties);
        }
        
        public static void SetPropertyBlockAndNotify(Renderer renderer, MaterialPropertyBlock properties, int materialIndex)
        {
            renderer.SetPropertyBlock(properties, materialIndex);
            OnPropertyBlockMaterialIndexChanged(renderer, properties, materialIndex);
        }
    }
}