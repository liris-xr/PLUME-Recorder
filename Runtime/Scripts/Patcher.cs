using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PLUME
{
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
            Debug.Log("Applied Harmony patches.");
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
                RendererEvents.OnSetSharedMaterials.Invoke(__instance, value);
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
    
    [HarmonyPatch(typeof(Object), nameof(Object.Instantiate), typeof(Object), typeof(Vector3), typeof(Quaternion))]
    public class PatchGameObjectInstantiate01
    {
        public static void Postfix(Object __result)
        {
            if (ObjectEvents.OnCreate != null)
                ObjectEvents.OnCreate.Invoke(__result);
        }
    }

    [HarmonyPatch(typeof(Object), nameof(Object.Instantiate), typeof(Object), typeof(Vector3), typeof(Quaternion),
        typeof(Transform))]
    public class PatchGameObjectInstantiate02
    {
        public static void Postfix(Transform parent, Object __result)
        {
            // If parent is null, Object.Instantiate(original, position, rotation) is called instead. We don't want the event to be fired twice.
            if (parent != null)
            {
                if (ObjectEvents.OnCreate != null)
                    ObjectEvents.OnCreate.Invoke(__result);
            }
        }
    }

    [HarmonyPatch(typeof(Object), nameof(Object.Instantiate), typeof(Object))]
    public class PatchGameObjectInstantiate03
    {
        public static void Postfix(Object __result)
        {
            if (ObjectEvents.OnCreate != null)
                ObjectEvents.OnCreate.Invoke(__result);
        }
    }

    [HarmonyPatch(typeof(Object), nameof(Object.Instantiate), typeof(Object), typeof(Transform), typeof(bool))]
    public class PatchGameObjectInstantiate04
    {
        public static void Postfix(Transform parent, Object __result)
        {
            // If parent is null, Object.Instantiate(original) is called instead. We don't want the event to be fired twice.
            if (parent != null)
            {
                if (ObjectEvents.OnCreate != null)
                    ObjectEvents.OnCreate.Invoke(__result);
            }
        }
    }

    [HarmonyPatch]
    public class PatchGameObjectInstantiate05
    {
        static MethodBase TargetMethod()
        {
            var method = (from m in typeof(Object).GetMethods()
                where m.Name == nameof(Object.Instantiate) && m.IsGenericMethodDefinition &&
                      m.GetParameters().Length == 1
                select m).Single().MakeGenericMethod(typeof(Object));

            return method;
        }

        public static void Postfix(Object __result)
        {
            if (ObjectEvents.OnCreate != null)
                ObjectEvents.OnCreate.Invoke(__result);
        }
    }

    [HarmonyPatch(typeof(Object), "set_" + nameof(Object.name), typeof(string))]
    public class PatchObjectSetName
    {
        public static void Prefix(Object __instance, string value)
        {
            if (ObjectEvents.OnSetName != null)
                ObjectEvents.OnSetName.Invoke(__instance, value);
        }
    }

    [HarmonyPatch(typeof(Object), nameof(Object.Destroy), typeof(Object))]
    public class PatchObjectDestroy
    {
        public static void Prefix(Object obj)
        {
            if (ObjectEvents.OnDestroy != null)
                ObjectEvents.OnDestroy.Invoke(obj);
        }
    }

    [HarmonyPatch(typeof(Object), nameof(Object.DestroyImmediate), typeof(Object))]
    public class PatchObjectDestroyImmediate
    {
        public static void Prefix(Object obj)
        {
            if (ObjectEvents.OnDestroy != null)
                ObjectEvents.OnDestroy.Invoke(obj);
        }
    }

    [HarmonyPatch(typeof(GameObject), nameof(GameObject.AddComponent), typeof(Type))]
    public class PatchGameObjectAddComponent
    {
        public static void Postfix(GameObject __instance, Type componentType, Component __result)
        {
            if (ObjectEvents.OnCreate != null)
                ObjectEvents.OnCreate(__result);
        }
    }

    [HarmonyPatch(typeof(GameObject), MethodType.Constructor)]
    public class PatchGameObjectConstructor01
    {
        public static void Postfix(GameObject __instance)
        {
            if (ObjectEvents.OnCreate != null)
                ObjectEvents.OnCreate(__instance);
        }
    }

    [HarmonyPatch(typeof(GameObject), MethodType.Constructor, typeof(string))]
    public class PatchGameObjectConstructor02
    {
        public static void Postfix(GameObject __instance)
        {
            if (ObjectEvents.OnCreate != null)
                ObjectEvents.OnCreate(__instance);
        }
    }

    [HarmonyPatch(typeof(GameObject), MethodType.Constructor, typeof(string), typeof(Type[]))]
    public class PatchGameObjectConstructor03
    {
        public static void Postfix(GameObject __instance)
        {
            if (ObjectEvents.OnCreate != null)
                ObjectEvents.OnCreate(__instance);
        }
    }
}