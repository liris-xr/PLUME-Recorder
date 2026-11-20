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

        public delegate void OnBeforeTransformReplacedDelegate(GameObject go);

        public static event OnCreatedDelegate OnCreated = delegate { };
        public static event OnComponentAddedDelegate OnComponentAdded = delegate { };
        public static event OnActiveChangedDelegate OnActiveChanged = delegate { };
        public static event OnTagChangedDelegate OnTagChanged = delegate { };

        /// <summary>
        /// Raised before a component is added that makes Unity replace the GameObject's Transform by a
        /// RectTransform (e.g. a UI Image). A Transform in a TransformAccessArray must be removed from it before:
        /// replacing it corrupts Unity's native state, which then crashes when the GameObject is destroyed.
        /// </summary>
        public static event OnBeforeTransformReplacedDelegate OnBeforeTransformReplaced = delegate { };

        // Temporary list to avoid allocations when calling GetComponentsInChildren
        private static readonly List<Component> TempComponents = new();

        private static readonly Dictionary<Type, bool> RequiresRectTransformByType = new();

        internal static void NotifyCreated(GameObject go)
        {
            try
            {
                OnCreated(go);
            }
            catch (Exception e)
            {
                HookErrors.Report($"{nameof(GameObjectHooks)}.{nameof(OnCreated)}",
                    "Failed to record a created GameObject. The application is not affected.", e);
            }
        }

        internal static void NotifyComponentAdded(GameObject go, Component component)
        {
            try
            {
                OnComponentAdded(go, component);
            }
            catch (Exception e)
            {
                HookErrors.Report($"{nameof(GameObjectHooks)}.{nameof(OnComponentAdded)}",
                    "Failed to record an added component. The application is not affected.", e);
            }
        }

        internal static void NotifyBeforeTransformReplaced(GameObject go)
        {
            try
            {
                OnBeforeTransformReplaced(go);
            }
            catch (Exception e)
            {
                HookErrors.Report($"{nameof(GameObjectHooks)}.{nameof(OnBeforeTransformReplaced)}",
                    "Failed to prepare for a Transform replaced by a RectTransform. The application is not affected.",
                    e);
            }
        }

        internal static void NotifyActiveChanged(GameObject go, bool active)
        {
            try
            {
                OnActiveChanged(go, active);
            }
            catch (Exception e)
            {
                HookErrors.Report($"{nameof(GameObjectHooks)}.{nameof(OnActiveChanged)}",
                    "Failed to record a GameObject active state change. The application is not affected.", e);
            }
        }

        internal static void NotifyTagChanged(GameObject go, string tag)
        {
            try
            {
                OnTagChanged(go, tag);
            }
            catch (Exception e)
            {
                HookErrors.Report($"{nameof(GameObjectHooks)}.{nameof(OnTagChanged)}",
                    "Failed to record a GameObject tag change. The application is not affected.", e);
            }
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
            // A null or destroyed GameObject is left to the user's call, so that Unity raises its own exception.
            if (go == null)
            {
                go.SetActive(active);
                return;
            }

            var previousActive = go.activeSelf;
            go.SetActive(active);
            if (previousActive == active)
                return;
            NotifyActiveChanged(go, active);
        }

        public static void SetTagPropertyAndNotify(GameObject go, string tag)
        {
            if (go == null)
            {
                go.tag = tag;
                return;
            }

            // Not CompareTag before the call: the user's call must always run and be the one to reject an invalid tag.
            var previousTag = go.tag;
            go.tag = tag;
            if (previousTag != tag)
                NotifyTagChanged(go, tag);
        }

        public static Component AddComponentAndNotify(GameObject go, Type componentType)
        {
            // Required components cannot be looked up without both arguments: the user's call raises Unity's error.
            if (go == null || componentType == null)
                return go.AddComponent(componentType);

            var missingRequiredTypes = GetMissingRequiredComponents(go, componentType);

            if (go.transform is not RectTransform && RequiresRectTransform(componentType))
                NotifyBeforeTransformReplaced(go);

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
            if (go == null)
                return go.AddComponent<T>();

            var missingRequiredTypes = GetMissingRequiredComponents(go, typeof(T));

            if (go.transform is not RectTransform && RequiresRectTransform(typeof(T)))
                NotifyBeforeTransformReplaced(go);

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

        /// <summary>
        /// Whether adding a component of the given type gives the GameObject a RectTransform, directly or through
        /// the components it requires.
        /// </summary>
        private static bool RequiresRectTransform(Type componentType)
        {
            if (componentType == typeof(RectTransform))
                return true;

            if (RequiresRectTransformByType.TryGetValue(componentType, out var requires))
                return requires;

            // Set before recursing, so that requirement cycles end.
            RequiresRectTransformByType[componentType] = false;

            foreach (var attr in componentType.GetCustomAttributes<RequireComponent>())
            {
                requires |= attr.m_Type0 != null && RequiresRectTransform(attr.m_Type0) ||
                            attr.m_Type1 != null && RequiresRectTransform(attr.m_Type1) ||
                            attr.m_Type2 != null && RequiresRectTransform(attr.m_Type2);
            }

            return RequiresRectTransformByType[componentType] = requires;
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