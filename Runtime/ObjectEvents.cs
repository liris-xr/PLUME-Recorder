using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PLUME
{
    public static class ObjectEvents
    {
        public static Action<Object, string> OnSetName;

        public static Action<Object> OnCreate;

        public static Action<int> OnDestroy;
    }

    public static class RendererEvents
    {
        public static Action<Renderer, Material> OnSetInstanceMaterial;

        public static Action<Renderer, Material[]> OnSetInstanceMaterials;

        public static Action<Renderer, Material> OnSetSharedMaterial;

        public static Action<Renderer, Material[]> OnSetSharedMaterials;
    }
}