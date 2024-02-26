using System;
using System.Diagnostics.CodeAnalysis;
using PLUME.Core;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = PLUME.Core.Logger;

namespace PLUME.Base.Hooks
{
    public static class GameObjectHooks
    {
        public static Action<GameObject> OnCreate;

        public static Action<GameObject, Component> OnAddComponent;

        public static Action<GameObject, bool> OnSetActive;

        public static Action<GameObject, string> OnSetTag;

        [RegisterConstructorHook(typeof(GameObject))]
        public static void ConstructorHook(GameObject go)
        {
            if (go != null)
            {
                OnCreate?.Invoke(go);
            }
        }

        [RegisterConstructorHook(typeof(GameObject), typeof(string))]
        public static void ConstructorHook(string name, GameObject go)
        {
            OnCreate?.Invoke(go);
        }

        [RegisterConstructorHook(typeof(GameObject), typeof(string), typeof(Type[]))]
        public static void ConstructorHook(string name, Type[] components, GameObject go)
        {
            OnCreate?.Invoke(go);
        }

        [RegisterMethodHook(typeof(GameObject), nameof(GameObject.CreatePrimitive), typeof(PrimitiveType))]
        public static void CreatePrimitiveHook(PrimitiveType type, GameObject go)
        {
            OnCreate?.Invoke(go);
        }

        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        [RegisterMethodHook(typeof(GameObject), nameof(GameObject.AddComponent), typeof(Type))]
        public static void AddComponentHook(GameObject go, Type componentType, Component component)
        {
            if (component != null)
            {
                OnAddComponent?.Invoke(go, component);
            }
        }

        [RegisterMethodHook(typeof(GameObject), nameof(GameObject.AddComponent))]
        public static void AddComponentHook(GameObject go, Component component)
        {
            if (component != null)
            {
                OnAddComponent?.Invoke(go, component);
            }
        }

        [RegisterMethodHook(typeof(GameObject), nameof(GameObject.SetGameObjectsActive), typeof(NativeArray<int>),
            typeof(bool))]
        public static void SetGameObjectsActiveHook(NativeArray<int> instanceIDs, bool active)
        {
            // TODO
            Logger.LogWarning("SetGameObjectsActiveHook not implemented");
        }

        [RegisterMethodHook(typeof(GameObject), nameof(GameObject.SetGameObjectsActive), typeof(ReadOnlySpan<int>),
            typeof(bool))]
        public static void SetGameObjectsActiveHook(ReadOnlySpan<int> instanceIDs, bool active)
        {
            // TODO
            Logger.LogWarning("SetGameObjectsActiveHook not implemented");
        }

        [RegisterMethodHook(typeof(GameObject), nameof(GameObject.InstantiateGameObjects), typeof(int), typeof(int),
            typeof(NativeArray<int>), typeof(NativeArray<int>), typeof(Scene))]
        public static void InstantiateGameObjectsHook(int sourceInstanceID, int count, NativeArray<int> newInstanceIDs,
            NativeArray<int> newTransformInstanceIDs, Scene destinationScene)
        {
            // TODO
            Logger.LogWarning("InstantiateGameObjectsHook not implemented");
        }
        
        [RegisterMethodHook(typeof(GameObject), nameof(GameObject.SetActive), typeof(bool))]
        public static void SetActiveHook(GameObject go, bool active)
        {
            OnSetActive?.Invoke(go, active);
        }

        [RegisterPropertySetterHook(typeof(GameObject), nameof(GameObject.tag))]
        public static void SetTagHook(GameObject go, string tag)
        {
            OnSetTag?.Invoke(go, tag);
        }
    }
}