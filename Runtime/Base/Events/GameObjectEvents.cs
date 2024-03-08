using System;
using System.Collections.Generic;
using System.Reflection;
using PLUME.Core;
using UnityEngine;
using UnityEngine.Scripting;

namespace PLUME.Base.Events
{
    public static class GameObjectEvents
    {
        public delegate void OnCreatedDelegate(GameObject go);

        public delegate void OnComponentAddedDelegate(GameObject go, Component component);

        public delegate void OnActiveChangedDelegate(GameObject go, bool active);

        public delegate void OnTagChangedDelegate(GameObject go, string tag);

        public static event OnCreatedDelegate OnCreated = delegate { };
        public static event OnComponentAddedDelegate OnComponentAdded = delegate { };
        public static event OnActiveChangedDelegate OnActiveChanged = delegate { };
        public static event OnTagChangedDelegate OnTagChanged = delegate { };

        // Temporary list to avoid allocations when calling GetComponentsInChildren
        private static readonly List<Component> TempComponents = new();

        internal static void NotifyCreated(GameObject go)
        {
            OnCreated(go);
        }

        internal static void NotifyComponentAdded(GameObject go, Component component)
        {
            OnComponentAdded(go, component);
        }

        [Preserve]
        [RegisterConstructorDetour(typeof(GameObject))]
        public static GameObject CreateAndNotify()
        {
            var go = new GameObject();
            NotifyCreated(go);
            return go;
        }

        [Preserve]
        [RegisterConstructorDetour(typeof(GameObject), typeof(string))]
        public static GameObject CreateAndNotify(string name)
        {
            var go = new GameObject(name);
            NotifyCreated(go);
            return go;
        }

        [Preserve]
        [RegisterConstructorDetour(typeof(GameObject), typeof(string), typeof(Type[]))]
        public static GameObject CreateAndNotify(string name, Type[] components)
        {
            var go = new GameObject(name, components);
            NotifyCreated(go);

            go.GetComponents(TempComponents);

            foreach (var component in TempComponents)
            {
                NotifyComponentAdded(go, component);
            }

            return go;
        }

        [Preserve]
        [RegisterMethodDetour(typeof(GameObject), nameof(GameObject.CreatePrimitive), typeof(PrimitiveType))]
        public static GameObject CreatePrimitiveAndNotify(PrimitiveType type)
        {
            var go = GameObject.CreatePrimitive(type);
            NotifyCreated(go);

            go.GetComponents(TempComponents);

            foreach (var component in TempComponents)
            {
                NotifyComponentAdded(go, component);
            }

            return go;
        }

        [Preserve]
        [RegisterMethodDetour(typeof(GameObject), nameof(GameObject.SetActive), typeof(bool))]
        public static void SetActiveAndNotify(GameObject go, bool active)
        {
            var previousActive = go.activeSelf;
            go.SetActive(active);
            if (previousActive == active)
                return;
            OnActiveChanged(go, active);
        }

        [Preserve]
        [RegisterPropertySetterDetour(typeof(GameObject), nameof(GameObject.tag))]
        public static void SetTagPropertyAndNotify(GameObject go, string tag)
        {
            if (go.CompareTag(tag)) return;
            go.tag = tag;
            OnTagChanged(go, tag);
        }

        [Preserve]
        [RegisterMethodDetour(typeof(GameObject), nameof(GameObject.AddComponent), typeof(Type))]
        public static Component AddComponentAndNotify(GameObject go, Type componentType)
        {
            var missingRequiredTypes = GetMissingRequiredComponents(go, componentType);

            var component = go.AddComponent(componentType);

            if (component != null)
            {
                NotifyComponentAdded(go, component);
                AddMissingRequiredComponents(go, missingRequiredTypes);
            }

            return component;
        }

        [Preserve]
        [RegisterMethodDetour(typeof(GameObject), nameof(GameObject.AddComponent))]
        public static T AddComponentAndNotify<T>(GameObject go) where T : Component
        {
            var missingRequiredTypes = GetMissingRequiredComponents(go, typeof(T));

            var component = go.AddComponent<T>();

            if (component != null)
            {
                NotifyComponentAdded(go, component);
                AddMissingRequiredComponents(go, missingRequiredTypes);
            }

            return component;
        }

        private static void AddMissingRequiredComponents(GameObject go, List<Type> missingRequiredTypes)
        {
            foreach (var missingRequiredType in missingRequiredTypes)
            {
                var implicitlyCreatedComponent = go.GetComponent(missingRequiredType);

                if (implicitlyCreatedComponent != null)
                {
                    NotifyComponentAdded(go, implicitlyCreatedComponent);
                }
            }
        }

        private static List<Type> GetMissingRequiredComponents(GameObject go, Type componentType)
        {
            var requireComponentsAttr = componentType.GetCustomAttributes<RequireComponent>();
            var missingRequiredTypes = new List<Type>();

            foreach (var attr in requireComponentsAttr)
            {
                if (attr.m_Type0 != null && go.GetComponent(attr.m_Type0) == null)
                {
                    missingRequiredTypes.Add(attr.m_Type0);
                }

                if (attr.m_Type1 != null && go.GetComponent(attr.m_Type1) == null)
                {
                    missingRequiredTypes.Add(attr.m_Type1);
                }

                if (attr.m_Type2 != null && go.GetComponent(attr.m_Type2) == null)
                {
                    missingRequiredTypes.Add(attr.m_Type2);
                }
            }

            return missingRequiredTypes;
        }
    }
}