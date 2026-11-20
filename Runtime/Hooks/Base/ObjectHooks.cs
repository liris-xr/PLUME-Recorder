using System;
using System.Linq;
using PLUME.Core.Hooks;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Scripting;
using Object = UnityEngine.Object;

namespace PLUME.Base.Hooks
{
    [Preserve]
    public class ObjectHooks : IRegisterHooksCallback
    {
        public delegate void OnNameChangedDelegate(Object obj, string name);

        public delegate void OnBeforeDestroyDelegate(Object obj, bool immediate);

        public delegate void OnDontDestroyOnLoadDelegate(Object obj);

        public static event OnNameChangedDelegate OnNameChanged = delegate { };
        public static event OnBeforeDestroyDelegate OnBeforeDestroyed = delegate { };
        public static event OnDontDestroyOnLoadDelegate OnDontDestroyOnLoad = delegate { };

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
                typeof(ObjectHooks).GetMethod(nameof(DontDestroyOnLoadAndNotify), new[] { typeof(Object) }),
                typeof(Object).GetMethod(nameof(Object.DontDestroyOnLoad), new[] { typeof(Object) })
            );

            hooksRegistry.RegisterHook(
                typeof(ObjectHooks).GetMethod(nameof(InstantiateAndNotify), new[] { typeof(Object) }),
                typeof(Object).GetMethod(nameof(Object.Instantiate), new[] { typeof(Object) })
            );

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

        private static void NotifyNameChanged(Object obj, string name)
        {
            try
            {
                OnNameChanged(obj, name);
            }
            catch (Exception e)
            {
                HookErrors.Report($"{nameof(ObjectHooks)}.{nameof(OnNameChanged)}",
                    "Failed to record an object name change. The application is not affected.", e);
            }
        }

        private static void NotifyDontDestroyOnLoad(Object obj)
        {
            try
            {
                OnDontDestroyOnLoad(obj);
            }
            catch (Exception e)
            {
                HookErrors.Report($"{nameof(ObjectHooks)}.{nameof(OnDontDestroyOnLoad)}",
                    "Failed to record a DontDestroyOnLoad call. The application is not affected.", e);
            }
        }

        private static void NotifyBeforeDestroyed(Object obj, bool immediate)
        {
            try
            {
                OnBeforeDestroyed(obj, immediate);
            }
            catch (Exception e)
            {
                HookErrors.Report($"{nameof(ObjectHooks)}.{nameof(OnBeforeDestroyed)}",
                    "Failed to record a destroyed object. The application is not affected.", e);
            }
        }

        private static void NotifyHierarchyBeforeDestroyed(Object obj, bool immediate)
        {
            // A null or already destroyed object has nothing left to record, and enumerating it would throw.
            if (obj == null)
                return;

            if (obj is GameObject go)
            {
                using var _ = ListPool<Component>.Get(out var tmpComponents);
                go.GetComponentsInChildren(tmpComponents);

                // Notified one by one, so that a failure for one component does not skip the others.
                foreach (var component in tmpComponents)
                {
                    NotifyBeforeDestroyed(component, immediate);
                }
            }

            NotifyBeforeDestroyed(obj, immediate);
        }

        public static void DontDestroyOnLoadAndNotify(Object obj)
        {
            // A destroyed GameObject goes to the else branch, so that Unity's own call raises the error instead of
            // reading its scene.
            if (obj is GameObject go && go != null)
            {
                var oldScene = go.scene;
                Object.DontDestroyOnLoad(obj);
                var scene = go.scene;
                SceneManagerHooks.NotifyGameObjectMovedToScene(go, oldScene, scene);
            }
            else
            {
                Object.DontDestroyOnLoad(obj);
            }

            NotifyDontDestroyOnLoad(obj);
        }

        public static void SetNamePropertyAndNotify(Object obj, string name)
        {
            // A null or destroyed object is left to the user's call, so that Unity raises its own exception.
            if (obj == null)
            {
                obj.name = name;
                return;
            }

            var previousName = obj.name;
            obj.name = name;
            if (name != previousName)
                NotifyNameChanged(obj, name);
        }

        public static void DestroyAndNotify(Object obj)
        {
            NotifyHierarchyBeforeDestroyed(obj, false);
            Object.Destroy(obj);
        }

        public static void DestroyAndNotify(Object obj, float t)
        {
            NotifyHierarchyBeforeDestroyed(obj, false);
            Object.Destroy(obj, t);
        }

        public static void DestroyImmediateAndNotify(Object obj)
        {
            NotifyHierarchyBeforeDestroyed(obj, true);
            Object.DestroyImmediate(obj);
        }

        public static void DestroyImmediateAndNotify(Object obj, bool allowDestroyingAssets)
        {
            NotifyHierarchyBeforeDestroyed(obj, true);
            Object.DestroyImmediate(obj, allowDestroyingAssets);
        }

        internal static void NotifyInstantiated(Object obj)
        {
            if (obj is GameObject go)
            {
                GameObjectHooks.NotifyCreated(go);

                using var _ = ListPool<Component>.Get(out var tmpComponents);
                go.GetComponentsInChildren(true, tmpComponents);

                foreach (var component in tmpComponents)
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