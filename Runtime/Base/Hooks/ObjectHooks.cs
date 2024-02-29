using System.Collections.Generic;
using PLUME.Core;
using UnityEngine;
using UnityEngine.Scripting;

namespace PLUME.Base.Hooks
{
    public static class ObjectHooks
    {
        public static System.Action<Object, string> OnSetName;

        public static System.Action<Object> OnBeforeDestroy;
        
        private static List<Component> _tmpComponentsList = new();

        [Preserve]
        [RegisterHookAfterPropertySetter(typeof(Object), nameof(Object.name))]
        public static void SetNameHook(Object obj, string name)
        {
            OnSetName?.Invoke(obj, name);
        }

        [Preserve]
        [RegisterHookBeforeMethod(typeof(Object), nameof(Object.Destroy), typeof(Object))]
        public static void BeforeDestroyHook(Object obj)
        {
            if (obj is GameObject go)
            {
                go.GetComponentsInChildren(true, _tmpComponentsList);
                
                foreach (var component in _tmpComponentsList)
                {
                    if (component is Transform t)
                    {
                        OnBeforeDestroy?.Invoke(t.gameObject);
                    }
                    
                    OnBeforeDestroy?.Invoke(component);
                }
            }
            
            if (obj != null)
                OnBeforeDestroy?.Invoke(obj);
        }

        [Preserve]
        [RegisterHookBeforeMethod(typeof(Object), nameof(Object.DestroyImmediate), typeof(Object))]
        public static void BeforeDestroyImmediateHook(Object obj)
        {
            if (obj is GameObject go)
            {
                go.GetComponentsInChildren(true, _tmpComponentsList);
                
                foreach (var component in _tmpComponentsList)
                {
                    if (component is Transform t)
                    {
                        OnBeforeDestroy?.Invoke(t.gameObject);
                    }
                    
                    OnBeforeDestroy?.Invoke(component);
                }
            }
            
            if (obj != null)
                OnBeforeDestroy?.Invoke(obj);
        }
    }
}