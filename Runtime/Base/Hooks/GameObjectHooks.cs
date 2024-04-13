using System;
using System.Collections.Generic;
using System.Reflection;
using PLUME.Core.Hooks;
using UnityEngine;
using UnityEngine.Scripting;

namespace PLUME.Base.Hooks
{
    [Preserve]
    public class GameObjectHooks : IRegisterHooksCallback
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

        public void RegisterHooks(HooksRegistry hooksRegistry)
        {
            hooksRegistry.RegisterHook(
                typeof(GameObjectHooks).GetMethod(nameof(CreateAndNotify), Type.EmptyTypes),
                typeof(GameObject).GetConstructor(Type.EmptyTypes)
            );

            hooksRegistry.RegisterHook(
                typeof(GameObjectHooks).GetMethod(nameof(CreateAndNotify), new[] { typeof(string) }),
                typeof(GameObject).GetConstructor(new[] { typeof(string) })
            );

            hooksRegistry.RegisterHook(
                typeof(GameObjectHooks).GetMethod(nameof(CreateAndNotify), new[] { typeof(string), typeof(Type[]) }),
                typeof(GameObject).GetConstructor(new[] { typeof(string), typeof(Type[]) })
            );
            
            hooksRegistry.RegisterHook(
                typeof(GameObjectHooks).GetMethod(nameof(CreatePrimitiveAndNotify), new[] { typeof(PrimitiveType) }),
                typeof(GameObject).GetMethod(nameof(GameObject.CreatePrimitive), new[] { typeof(PrimitiveType) })
            );
            
            hooksRegistry.RegisterHook(
                typeof(GameObjectHooks).GetMethod(nameof(SetTagPropertyAndNotify), new[] { typeof(GameObject), typeof(string) }),
                typeof(GameObject).GetProperty(nameof(GameObject.tag))!.GetSetMethod()
            );
            
            hooksRegistry.RegisterHook(
                typeof(GameObjectHooks).GetMethod(nameof(SetActiveAndNotify), new[] { typeof(GameObject), typeof(bool) }),
                typeof(GameObject).GetMethod(nameof(GameObject.SetActive), new[] { typeof(bool) })
            );
            
            hooksRegistry.RegisterHook(
                typeof(GameObjectHooks).GetMethod(nameof(AddComponentAndNotify), new[] { typeof(GameObject), typeof(Type) }),
                typeof(GameObject).GetMethod(nameof(GameObject.AddComponent), new[] { typeof(Type) })
            );
            
            hooksRegistry.RegisterHook(
                typeof(GameObjectHooks).GetMethod(nameof(AddComponentAndNotify), new[] { typeof(GameObject) }),
                typeof(GameObject).GetMethod(nameof(GameObject.AddComponent), Type.EmptyTypes)
            );
        }

        public static GameObject CreateAndNotify()
        {
            var go = new GameObject();
            NotifyCreated(go);
            return go;
        }

        public static GameObject CreateAndNotify(string name)
        {
            var go = new GameObject(name);
            NotifyCreated(go);
            return go;
        }

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

        public static void SetActiveAndNotify(GameObject go, bool active)
        {
            var previousActive = go.activeSelf;
            go.SetActive(active);
            if (previousActive == active)
                return;
            OnActiveChanged(go, active);
        }

        public static void SetTagPropertyAndNotify(GameObject go, string tag)
        {
            if (go.CompareTag(tag)) return;
            go.tag = tag;
            OnTagChanged(go, tag);
        }

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