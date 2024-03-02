using System.Collections.Generic;
using PLUME.Core;
using UnityEngine;
using UnityEngine.Scripting;

namespace PLUME.Base.Events
{
    public static class ObjectEvents
    {
        public delegate void OnNameChangedDelegate(Object obj, string name);

        public static event OnNameChangedDelegate OnNameChanged = delegate { };
        
        public delegate void OnBeforeDestroyDelegate(Object obj, bool immediate);

        public static event OnBeforeDestroyDelegate OnBeforeDestroyed = delegate { };
        
        // Temporary list to avoid allocations when calling GetComponentsInChildren
        private static readonly List<Component> TempComponents = new();

        [Preserve]
        [RegisterPropertySetterDetour(typeof(Object), nameof(Object.name))]
        public static void SetNamePropertyAndNotify(Object obj, string name)
        {
            var previousName = obj.name;
            obj.name = name;
            if (name != previousName)
                OnNameChanged(obj, name);
        }
        
        [Preserve]
        [RegisterMethodDetour(typeof(Object), nameof(Object.Destroy), typeof(Object))]
        public static void DestroyAndNotify(Object obj)
        {
            if (obj is GameObject go)
            {
                go.GetComponentsInChildren(TempComponents);
                
                foreach (var component in TempComponents)
                {
                    OnBeforeDestroyed(component, false);
                }
            }
            
            OnBeforeDestroyed(obj, false);
            Object.Destroy(obj);
        }
        
        [Preserve]
        [RegisterMethodDetour(typeof(Object), nameof(Object.Destroy), typeof(Object), typeof(float))]
        public static void DestroyAndNotify(Object obj, float t)
        {
            if (obj is GameObject go)
            {
                go.GetComponentsInChildren(TempComponents);
                
                foreach (var component in TempComponents)
                {
                    OnBeforeDestroyed(component, false);
                }
            }
            
            OnBeforeDestroyed(obj, false);
            Object.Destroy(obj, t);
        }
        
        [Preserve]
        [RegisterMethodDetour(typeof(Object), nameof(Object.DestroyImmediate), typeof(Object))]
        public static void DestroyImmediateAndNotify(Object obj)
        {
            if (obj is GameObject go)
            {
                go.GetComponentsInChildren(TempComponents);
                
                foreach (var component in TempComponents)
                {
                    OnBeforeDestroyed(component, true);
                }
            }
            
            OnBeforeDestroyed(obj, true);
            Object.DestroyImmediate(obj);
        }
        
        [Preserve]
        [RegisterMethodDetour(typeof(Object), nameof(Object.DestroyImmediate), typeof(Object), typeof(bool))]
        public static void DestroyImmediateAndNotify(Object obj, bool allowDestroyingAssets)
        {
            if (obj is GameObject go)
            {
                go.GetComponentsInChildren(TempComponents);
                
                foreach (var component in TempComponents)
                {
                    OnBeforeDestroyed(component, true);
                }
            }
            
            OnBeforeDestroyed(obj, true);
            Object.DestroyImmediate(obj, allowDestroyingAssets);
        }
    }
}