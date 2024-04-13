using System.Collections.Generic;
using System.Linq;
using PLUME.Core.Hooks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;
using Object = UnityEngine.Object;

namespace PLUME.Base.Hooks
{
    [Preserve]
    public class ObjectHooks : IRegisterHooksCallback
    {
        public delegate void OnNameChangedDelegate(Object obj, string name);

        public delegate void OnBeforeDestroyDelegate(Object obj, bool immediate);

        public static event OnNameChangedDelegate OnNameChanged = delegate { };
        public static event OnBeforeDestroyDelegate OnBeforeDestroyed = delegate { };

        // Temporary list to avoid allocations when calling GetComponentsInChildren
        private static readonly List<Component> TempComponents = new();

        public void RegisterHooks(HooksRegistry hooksRegistry)
        {
            hooksRegistry.RegisterHook(
                typeof(ObjectHooks).GetMethod(nameof(SetNamePropertyAndNotify)),
                typeof(Object).GetProperty(nameof(Object.name))?.GetSetMethod()
            );

            hooksRegistry.RegisterHook(
                typeof(ObjectHooks).GetMethod(nameof(DestroyAndNotify), new[] { typeof(Object) }),
                typeof(Object).GetMethod(nameof(Object.Destroy), new[] { typeof(Object) })
            );

            hooksRegistry.RegisterHook(
                typeof(ObjectHooks).GetMethod(nameof(DestroyAndNotify), new[] { typeof(Object), typeof(float) }),
                typeof(Object).GetMethod(nameof(Object.Destroy), new[] { typeof(Object), typeof(float) })
            );

            hooksRegistry.RegisterHook(
                typeof(ObjectHooks).GetMethod(nameof(DestroyImmediateAndNotify), new[] { typeof(Object) }),
                typeof(Object).GetMethod(nameof(Object.DestroyImmediate), new[] { typeof(Object) })
            );

            hooksRegistry.RegisterHook(
                typeof(ObjectHooks).GetMethod(nameof(DestroyImmediateAndNotify),
                    new[] { typeof(Object), typeof(bool) }),
                typeof(Object).GetMethod(nameof(Object.DestroyImmediate), new[] { typeof(Object), typeof(bool) })
            );

            hooksRegistry.RegisterHook(
                typeof(ObjectHooks).GetMethod(nameof(InstantiateAndNotify), new[] { typeof(Object) }),
                typeof(Object).GetMethod(nameof(Object.Instantiate), new[] { typeof(Object) })
            );

            var instantiateWithSceneParamMethod =
                typeof(Object).GetMethod(nameof(Object.Instantiate), new[] { typeof(Object), typeof(Scene) });

            // This method was introduced in one of the patch of Unity 2022. We need to check if it exists before adding a hook for it.
            if (instantiateWithSceneParamMethod != null)
            {
                hooksRegistry.RegisterHook(
                    typeof(ObjectHooks).GetMethod(nameof(InstantiateAndNotify),
                        new[] { typeof(Object), typeof(Scene) }),
                    instantiateWithSceneParamMethod
                );
            }

            hooksRegistry.RegisterHook(
                typeof(ObjectHooks).GetMethod(nameof(InstantiateAndNotify),
                    new[] { typeof(Object), typeof(Transform) }),
                typeof(Object).GetMethod(nameof(Object.Instantiate), new[] { typeof(Object), typeof(Transform) })
            );

            hooksRegistry.RegisterHook(
                typeof(ObjectHooks).GetMethod(nameof(InstantiateAndNotify),
                    new[] { typeof(Object), typeof(Transform), typeof(bool) }),
                typeof(Object).GetMethod(nameof(Object.Instantiate),
                    new[] { typeof(Object), typeof(Transform), typeof(bool) })
            );

            hooksRegistry.RegisterHook(
                typeof(ObjectHooks).GetMethod(nameof(InstantiateAndNotify),
                    new[] { typeof(Object), typeof(Vector3), typeof(Quaternion) }),
                typeof(Object).GetMethod(nameof(Object.Instantiate),
                    new[] { typeof(Object), typeof(Vector3), typeof(Quaternion) })
            );

            hooksRegistry.RegisterHook(
                typeof(ObjectHooks).GetMethod(nameof(InstantiateAndNotify),
                    new[] { typeof(Object), typeof(Vector3), typeof(Quaternion), typeof(Transform) }),
                typeof(Object).GetMethod(nameof(Object.Instantiate),
                    new[] { typeof(Object), typeof(Vector3), typeof(Quaternion), typeof(Transform) })
            );
            
            hooksRegistry.RegisterHook(
                typeof(ObjectHooks).GetMethods().First(m =>
                    m.IsGenericMethod && m.Name == nameof(InstantiateAndNotify)
                                      && m.GetParameters().Length == 1
                                      && typeof(Object).IsAssignableFrom(m.GetParameters()[0].ParameterType)),
                typeof(Object).GetMethods().First(m =>
                    m.IsGenericMethod && m.Name == nameof(Object.Instantiate)
                                      && m.GetParameters().Length == 1
                                      && typeof(Object).IsAssignableFrom(m.GetParameters()[0].ParameterType))
            );
            
            hooksRegistry.RegisterHook(
                typeof(ObjectHooks).GetMethods().First(m =>
                    m.IsGenericMethod && m.Name == nameof(InstantiateAndNotify)
                                      && m.GetParameters().Length == 2
                                      && typeof(Object).IsAssignableFrom(m.GetParameters()[0].ParameterType)
                                      && m.GetParameters()[1].ParameterType == typeof(Transform)),
                typeof(Object).GetMethods().First(m =>
                    m.IsGenericMethod && m.Name == nameof(Object.Instantiate)
                                      && m.GetParameters().Length == 2
                                      && typeof(Object).IsAssignableFrom(m.GetParameters()[0].ParameterType)
                                      && m.GetParameters()[1].ParameterType == typeof(Transform))
            );
            
            hooksRegistry.RegisterHook(
                typeof(ObjectHooks).GetMethods().First(m =>
                    m.IsGenericMethod && m.Name == nameof(InstantiateAndNotify)
                                      && m.GetParameters().Length == 3
                                      && typeof(Object).IsAssignableFrom(m.GetParameters()[0].ParameterType)
                                      && m.GetParameters()[1].ParameterType == typeof(Transform)
                                      && m.GetParameters()[2].ParameterType == typeof(bool)),
                typeof(Object).GetMethods().First(m =>
                    m.IsGenericMethod && m.Name == nameof(Object.Instantiate)
                                      && m.GetParameters().Length == 3
                                      && typeof(Object).IsAssignableFrom(m.GetParameters()[0].ParameterType)
                                      && m.GetParameters()[1].ParameterType == typeof(Transform)
                                      && m.GetParameters()[2].ParameterType == typeof(bool))
            );
            
            hooksRegistry.RegisterHook(
                typeof(ObjectHooks).GetMethods().First(m =>
                    m.IsGenericMethod && m.Name == nameof(InstantiateAndNotify)
                                      && m.GetParameters().Length == 3
                                      && typeof(Object).IsAssignableFrom(m.GetParameters()[0].ParameterType)
                                      && m.GetParameters()[1].ParameterType == typeof(Vector3)
                                      && m.GetParameters()[2].ParameterType == typeof(Quaternion)),
                typeof(Object).GetMethods().First(m =>
                    m.IsGenericMethod && m.Name == nameof(Object.Instantiate)
                                      && m.GetParameters().Length == 3
                                      && typeof(Object).IsAssignableFrom(m.GetParameters()[0].ParameterType)
                                      && m.GetParameters()[1].ParameterType == typeof(Vector3)
                                      && m.GetParameters()[2].ParameterType == typeof(Quaternion))
            );
            
            hooksRegistry.RegisterHook(
                typeof(ObjectHooks).GetMethods().First(m =>
                    m.IsGenericMethod && m.Name == nameof(InstantiateAndNotify)
                                      && m.GetParameters().Length == 4
                                      && typeof(Object).IsAssignableFrom(m.GetParameters()[0].ParameterType)
                                      && m.GetParameters()[1].ParameterType == typeof(Vector3)
                                      && m.GetParameters()[2].ParameterType == typeof(Quaternion)
                                      && m.GetParameters()[3].ParameterType == typeof(Transform)),
                typeof(Object).GetMethods().First(m =>
                    m.IsGenericMethod && m.Name == nameof(Object.Instantiate)
                                      && m.GetParameters().Length == 4
                                      && typeof(Object).IsAssignableFrom(m.GetParameters()[0].ParameterType)
                                      && m.GetParameters()[1].ParameterType == typeof(Vector3)
                                      && m.GetParameters()[2].ParameterType == typeof(Quaternion)
                                      && m.GetParameters()[3].ParameterType == typeof(Transform))
            );
        }

        public static void SetNamePropertyAndNotify(Object obj, string name)
        {
            var previousName = obj.name;
            obj.name = name;
            if (name != previousName)
                OnNameChanged(obj, name);
        }

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
                GameObjectHooks.NotifyCreated(go);

                go.GetComponentsInChildren(true, TempComponents);
                
                foreach (var component in TempComponents)
                {
                    if (component is Transform t)
                    {
                        GameObjectHooks.NotifyCreated(t.gameObject);
                    }
                    
                    GameObjectHooks.NotifyComponentAdded(component.gameObject, component);
                }
            }
            else if (obj is Component component)
            {
                GameObjectHooks.NotifyCreated(component.gameObject);
                GameObjectHooks.NotifyComponentAdded(component.gameObject, component);
            }
        }

        public static Object InstantiateAndNotify(Object original)
        {
            var instance = Object.Instantiate(original);
            NotifyInstantiated(instance);
            return instance;
        }

        public static Object InstantiateAndNotify(Object original, Transform parent)
        {
            var instance = Object.Instantiate(original, parent);
            NotifyInstantiated(instance);
            return instance;
        }

        public static Object InstantiateAndNotify(Object original, Scene scene)
        {
            var instance = Object.Instantiate(original, scene);
            NotifyInstantiated(instance);
            return instance;
        }

        public static Object InstantiateAndNotify(Object original, Transform parent, bool instantiateInWorldSpace)
        {
            var instance = Object.Instantiate(original, parent, instantiateInWorldSpace);
            NotifyInstantiated(instance);
            return instance;
        }

        public static Object InstantiateAndNotify(Object original, Vector3 position, Quaternion rotation)
        {
            var instance = Object.Instantiate(original, position, rotation);
            NotifyInstantiated(instance);
            return instance;
        }

        public static Object InstantiateAndNotify(Object original, Vector3 position, Quaternion rotation,
            Transform parent)
        {
            var instance = Object.Instantiate(original, position, rotation, parent);
            NotifyInstantiated(instance);
            return instance;
        }

        public static T InstantiateAndNotify<T>(T original) where T : Object
        {
            var instance = Object.Instantiate(original);
            NotifyInstantiated(instance);
            return instance;
        }

        public static T InstantiateAndNotify<T>(T original, Transform parent) where T : Object
        {
            var instance = Object.Instantiate(original, parent);
            NotifyInstantiated(instance);
            return instance;
        }

        public static T InstantiateAndNotify<T>(T original, Transform parent, bool instantiateInWorldSpace)
            where T : Object
        {
            var instance = Object.Instantiate(original, parent, instantiateInWorldSpace);
            NotifyInstantiated(instance);
            return instance;
        }

        public static T InstantiateAndNotify<T>(T original, Vector3 position, Quaternion rotation) where T : Object
        {
            var instance = Object.Instantiate(original, position, rotation);
            NotifyInstantiated(instance);
            return instance;
        }

        public static T InstantiateAndNotify<T>(T original, Vector3 position, Quaternion rotation, Transform parent)
            where T : Object
        {
            var instance = Object.Instantiate(original, position, rotation, parent);
            NotifyInstantiated(instance);
            return instance;
        }
    }
}