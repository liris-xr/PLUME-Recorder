using System.Collections.Generic;
using UnityEngine;

namespace PLUME.Base.Hooks
{
    public class TestHooks
    {
        public static void GetComponentsInChildrenDetour<T>(GameObject go, List<T> results)
        {
            Debug.Log("GameObject::GetComponentsInChildren Detour");
            go.GetComponentsInChildren(results);
        }

        public static T FindFirstObjectByTypeDetour<T>() where T : Object
        {
            Debug.Log("Object::FindFirstObjectByType Detour");
            return Object.FindFirstObjectByType<T>();
        }

        public static GameObject GameObjectCtorDetour(string name)
        {
            Debug.Log("GameObject::.ctor Detour");
            // TODO: call callback before
            var go = new GameObject(name);
            // TODO: call callback after
            return go;
        }

        public static T AddComponentDetour<T>(GameObject go) where T : Component
        {
            Debug.Log("GameObject::AddComponent Detour " + typeof(T) + " " + go.name);
            // TODO: call callback before
            var component = go.AddComponent<T>();
            // TODO: call callback after
            return component;
        }
    }
}