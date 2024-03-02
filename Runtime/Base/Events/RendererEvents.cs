using System.Collections.Generic;
using System.Linq;
using PLUME.Core;
using UnityEngine;
using UnityEngine.Scripting;

namespace PLUME.Base.Events
{
    public static class RendererEvents
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

        [Preserve]
        [RegisterPropertySetterDetour(typeof(Renderer), nameof(Renderer.enabled))]
        public static void SetEnabledAndNotify(Renderer renderer, bool enabled)
        {
            var previousEnabled = renderer.enabled;
            renderer.enabled = enabled;
            if (previousEnabled != enabled)
            {
                OnEnabledChanged(renderer, enabled);
            }
        }

        [Preserve]
        [RegisterPropertySetterDetour(typeof(Renderer), nameof(Renderer.material))]
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

        [Preserve]
        [RegisterPropertySetterDetour(typeof(Renderer), nameof(Renderer.materials))]
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

        [Preserve]
        [RegisterMethodDetour(typeof(Renderer), nameof(Renderer.SetMaterials), typeof(List<Material>))]
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

        [Preserve]
        [RegisterPropertySetterDetour(typeof(Renderer), nameof(Renderer.sharedMaterial))]
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

        [Preserve]
        [RegisterPropertySetterDetour(typeof(Renderer), nameof(Renderer.sharedMaterials))]
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

        [Preserve]
        [RegisterMethodDetour(typeof(Renderer), nameof(Renderer.SetSharedMaterials), typeof(List<Material>))]
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

        [Preserve]
        [RegisterMethodDetour(typeof(Renderer), nameof(Renderer.SetPropertyBlock), typeof(MaterialPropertyBlock))]
        public static void SetPropertyBlockAndNotify(Renderer renderer, MaterialPropertyBlock properties)
        {
            renderer.SetPropertyBlock(properties);
            OnPropertyBlockChanged(renderer, properties);
        }
        
        [Preserve]
        [RegisterMethodDetour(typeof(Renderer), nameof(Renderer.SetPropertyBlock), typeof(MaterialPropertyBlock), typeof(int))]
        public static void SetPropertyBlockAndNotify(Renderer renderer, MaterialPropertyBlock properties, int materialIndex)
        {
            renderer.SetPropertyBlock(properties, materialIndex);
            OnPropertyBlockMaterialIndexChanged(renderer, properties, materialIndex);
        }
    }
}