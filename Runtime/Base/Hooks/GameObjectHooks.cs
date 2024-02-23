using System;
using System.Diagnostics.CodeAnalysis;
using PLUME.Core;
using UnityEngine;

namespace PLUME.Base.Hooks
{
    public static class GameObjectHooks
    {
        public static Action<GameObject, string> OnSetName;

        public static Action<GameObject> OnCreate;

        public static Action<GameObject> OnDestroy;

        public static Action<GameObject, Component> OnAddComponent;

        [RegisterConstructorHook(typeof(GameObject))]
        public static void GameObjectConstructorHook(GameObject go)
        {
            if (go != null)
            {
                OnCreate?.Invoke(go);
                Debug.Log("New GameObject hook: " + go.name);
            }
        }

        [RegisterConstructorHook(typeof(GameObject), typeof(string))]
        public static void GameObjectConstructorHook(string name, GameObject go)
        {
            if (go != null)
            {
                OnCreate?.Invoke(go);
                Debug.Log("New GameObject hook: " + go.name);
            }
        }

        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        [RegisterMethodHook(typeof(GameObject), nameof(GameObject.AddComponent), typeof(Type))]
        public static void AddComponentHook(GameObject go, Type componentType, Component component)
        {
            if (component != null)
            {
                OnAddComponent?.Invoke(go, component);
                Debug.Log("New Component hook: " + go.name + " > " + component.GetType());
            }
        }

        [RegisterMethodHook(typeof(GameObject), nameof(GameObject.AddComponent))]
        public static void AddComponentHook(GameObject go, Component component)
        {
            if (component != null)
            {
                OnAddComponent?.Invoke(go, component);
                Debug.Log("New Component hook: " + go.name + " > " + component.GetType());
            }
        }
    }
}