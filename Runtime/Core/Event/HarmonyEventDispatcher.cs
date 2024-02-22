using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Logger = PLUME.Core.Logger;

namespace PLUME.Core.Event
{
#if UNITY_STANDALONE_WIN
    /**
     * Simple monkey patcher using Harmony.
     *
     * Help us create custom events for objects, preventing polling in recorders.
     */
    public static class Patcher
    {
        public static void DoPatching()
        {
            var harmony = new Harmony("fr.liris.plume.patch");
            harmony.PatchAll();
            Logger.Log("Applied Harmony patches.");
        }
    }

    [HarmonyPatch(typeof(Renderer), "set_" + nameof(Renderer.materials))]
    public class PatchRendererSetMaterials
    {
        public static void Postfix(Renderer __instance, Material[] value)
        {
            if (RendererEvents.OnSetInstanceMaterials != null)
                RendererEvents.OnSetInstanceMaterials.Invoke(__instance, value);
        }
    }

    [HarmonyPatch(typeof(Renderer), "set_" + nameof(Renderer.material))]
    public class PatchRendererSetMaterial
    {
        public static void Postfix(Renderer __instance, Material value)
        {
            if (RendererEvents.OnSetInstanceMaterial != null)
                RendererEvents.OnSetInstanceMaterial.Invoke(__instance, value);
        }
    }

    [HarmonyPatch(typeof(Renderer), "set_" + nameof(Renderer.sharedMaterials))]
    public class PatchRendererSetSharedMaterials
    {
        public static void Postfix(Renderer __instance, Material[] value)
        {
            if (RendererEvents.OnSetSharedMaterials != null)
                RendererEvents.OnSetSharedMaterials?.Invoke(__instance, value);
        }
    }

    [HarmonyPatch(typeof(Renderer), "set_" + nameof(Renderer.sharedMaterial))]
    public class PatchRendererSetSharedMaterial
    {
        public static void Postfix(Renderer __instance, Material value)
        {
            if (RendererEvents.OnSetSharedMaterial != null)
                RendererEvents.OnSetSharedMaterial.Invoke(__instance, value);
        }
    }
    
    public static class PatchInstantiateCommon {
        public static void OnInstantiateObject(UnityEngine.Object obj)
        {
            if (ObjectEvents.OnCreate == null) return;
            ObjectEvents.OnCreate.Invoke(obj);
            if (obj is not GameObject go) return;
            foreach (var childObj in go.GetComponentsInChildren<Component>())
            {
                if (childObj is Transform t)
                    ObjectEvents.OnCreate.Invoke(t.gameObject);
                ObjectEvents.OnCreate.Invoke(childObj);
            }
        }
    }

    [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Instantiate), typeof(UnityEngine.Object), typeof(Vector3), typeof(Quaternion))]
    public class PatchGameObjectInstantiate01
    {
        public static void Postfix(UnityEngine.Object __result)
        {
            PatchInstantiateCommon.OnInstantiateObject(__result);
        }
    }

    [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Instantiate), typeof(UnityEngine.Object), typeof(Vector3), typeof(Quaternion),
        typeof(Transform))]
    public class PatchGameObjectInstantiate02
    {
        public static void Postfix(Transform parent, UnityEngine.Object __result)
        {
            // If parent is null, Object.Instantiate(original, position, rotation) is called instead. We don't want the event to be fired twice.
            if (parent != null)
            {
                PatchInstantiateCommon.OnInstantiateObject(__result);
            }
        }
    }

    [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Instantiate), typeof(UnityEngine.Object))]
    public class PatchGameObjectInstantiate03
    {
        public static void Postfix(UnityEngine.Object __result)
        {
            PatchInstantiateCommon.OnInstantiateObject(__result);
        }
    }

    [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Instantiate), typeof(UnityEngine.Object), typeof(Transform), typeof(bool))]
    public class PatchGameObjectInstantiate04
    {
        public static void Postfix(Transform parent, UnityEngine.Object __result)
        {
            // If parent is null, Object.Instantiate(original) is called instead. We don't want the event to be fired twice.
            if (parent != null)
            {
                PatchInstantiateCommon.OnInstantiateObject(__result);
            }
        }
    }

    [HarmonyPatch]
    public class PatchGameObjectInstantiate05
    {
        static MethodBase TargetMethod()
        {
            var method = (from m in typeof(UnityEngine.Object).GetMethods()
                where m.Name == nameof(UnityEngine.Object.Instantiate) && m.IsGenericMethodDefinition &&
                      m.GetParameters().Length == 1
                select m).Single().MakeGenericMethod(typeof(UnityEngine.Object));

            return method;
        }

        public static void Postfix(UnityEngine.Object __result)
        {
            PatchInstantiateCommon.OnInstantiateObject(__result);
        }
    }

    [HarmonyPatch(typeof(UnityEngine.Object), "set_" + nameof(UnityEngine.Object.name), typeof(string))]
    public class PatchObjectSetName
    {
        public static void Prefix(UnityEngine.Object __instance, string value)
        {
            if (ObjectEvents.OnSetName != null)
                ObjectEvents.OnSetName.Invoke(__instance, value);
        }
    }

    [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Destroy), typeof(UnityEngine.Object))]
    public class PatchObjectDestroy
    {
        public static void Prefix(UnityEngine.Object obj)
        {
            if (ObjectEvents.OnDestroy != null && obj != null)
                ObjectEvents.OnDestroy.Invoke(obj.GetInstanceID());
        }
    }

    [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.DestroyImmediate), typeof(UnityEngine.Object))]
    public class PatchObjectDestroyImmediate
    {
        public static void Prefix(UnityEngine.Object obj)
        {
            if (ObjectEvents.OnDestroy != null && obj != null)
                ObjectEvents.OnDestroy.Invoke(obj.GetInstanceID());
        }
    }

    [HarmonyPatch(typeof(GameObject), nameof(GameObject.AddComponent), typeof(Type))]
    public class PatchGameObjectAddComponent
    {
        public static void Postfix(GameObject __instance, Type componentType, Component __result)
        {
            if (ObjectEvents.OnCreate != null)
                ObjectEvents.OnCreate.Invoke(__result);
        }
    }

    [HarmonyPatch(typeof(GameObject), MethodType.Constructor)]
    public class PatchGameObjectConstructor01
    {
        public static void Postfix(GameObject __instance)
        {
            if (ObjectEvents.OnCreate != null)
                ObjectEvents.OnCreate.Invoke(__instance);
        }
    }

    [HarmonyPatch(typeof(GameObject), MethodType.Constructor, typeof(string))]
    public class PatchGameObjectConstructor02
    {
        public static void Postfix(GameObject __instance)
        {
            if (ObjectEvents.OnCreate != null)
                ObjectEvents.OnCreate.Invoke(__instance);
        }
    }

    [HarmonyPatch(typeof(GameObject), MethodType.Constructor, typeof(string), typeof(Type[]))]
    public class PatchGameObjectConstructor03
    {
        public static void Postfix(GameObject __instance)
        {
            if (ObjectEvents.OnCreate != null)
                ObjectEvents.OnCreate.Invoke(__instance);
        }
    }
#endif
}