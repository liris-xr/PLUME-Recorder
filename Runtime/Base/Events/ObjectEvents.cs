using System.Collections.Generic;
using PLUME.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;

namespace PLUME.Base.Events
{
    public static class ObjectEvents
    {
        public delegate void OnNameChangedDelegate(Object obj, string name);

        public delegate void OnBeforeDestroyDelegate(Object obj, bool immediate);

        public static event OnNameChangedDelegate OnNameChanged = delegate { };
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

        internal static void NotifyInstantiated(Object obj)
        {
            if (obj is GameObject go)
            {
                GameObjectEvents.NotifyCreated(go);

                go.GetComponents(TempComponents);

                foreach (var component in TempComponents)
                {
                    GameObjectEvents.NotifyComponentAdded(go, component);
                }
            }
            else if (obj is Component component)
            {
                GameObjectEvents.NotifyCreated(component.gameObject);
                GameObjectEvents.NotifyComponentAdded(component.gameObject, component);
            }
        }

        [Preserve]
        [RegisterMethodDetour(typeof(Object), nameof(Object.Instantiate), typeof(Object))]
        public static Object InstantiateAndNotify(Object original)
        {
            var instance = Object.Instantiate(original);
            NotifyInstantiated(instance);
            return instance;
        }

        [Preserve]
        [RegisterMethodDetour(typeof(Object), nameof(Object.Instantiate), typeof(Object), typeof(Scene))]
        public static Object InstantiateAndNotify(Object original, Scene scene)
        {
            var instance = Object.Instantiate(original, scene);
            NotifyInstantiated(instance);
            return instance;
        }

        [Preserve]
        [RegisterMethodDetour(typeof(Object), nameof(Object.Instantiate), typeof(Object), typeof(Transform))]
        public static Object InstantiateAndNotify(Object original, Transform parent)
        {
            var instance = Object.Instantiate(original, parent);
            NotifyInstantiated(instance);
            return instance;
        }

        [Preserve]
        [RegisterMethodDetour(typeof(Object), nameof(Object.Instantiate), typeof(Object), typeof(Transform),
            typeof(bool))]
        public static Object InstantiateAndNotify(Object original, Transform parent, bool instantiateInWorldSpace)
        {
            var instance = Object.Instantiate(original, parent, instantiateInWorldSpace);
            NotifyInstantiated(instance);
            return instance;
        }

        [Preserve]
        [RegisterMethodDetour(typeof(Object), nameof(Object.Instantiate), typeof(Object), typeof(Vector3),
            typeof(Quaternion))]
        public static Object InstantiateAndNotify(Object original, Vector3 position, Quaternion rotation)
        {
            var instance = Object.Instantiate(original, position, rotation);
            NotifyInstantiated(instance);
            return instance;
        }

        [Preserve]
        [RegisterMethodDetour(typeof(Object), nameof(Object.Instantiate), typeof(Object), typeof(Vector3),
            typeof(Quaternion), typeof(Transform))]
        public static Object InstantiateAndNotify(Object original, Vector3 position, Quaternion rotation,
            Transform parent)
        {
            var instance = Object.Instantiate(original, position, rotation, parent);
            NotifyInstantiated(instance);
            return instance;
        }
    }
}