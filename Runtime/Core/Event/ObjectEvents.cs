using System;
using UnityEngine;

namespace PLUME.Core.Event
{
    public static class ObjectEvents
    {
        public static Action<UnityEngine.Object, string> OnSetName;

        public static Action<UnityEngine.Object> OnCreate;

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